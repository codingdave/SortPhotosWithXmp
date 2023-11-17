using MetadataExtractor;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Extensions;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByExif;

internal class RearrangeByExifRunner : IRun
{
    private readonly string _destinationDirectory;
    private readonly string _sourceDirectory;
    private readonly FilesFoundResult _filesFoundResult;
    private readonly FileOperationBase _fileOperation;
    private readonly DeleteFileOperation _deleteOperation;
    private readonly IFileScanner _fileScanner;
    private readonly IDirectory _directory;

    internal RearrangeByExifRunner(ILogger logger,
                             string sourceDirectory,
                             string destinationDirectory,
                             IFileScanner fileScanner,
                             IFile file,
                             IDirectory directory,
                             bool isMove,
                             bool isForce)
    {
        _sourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
        _destinationDirectory = destinationDirectory ?? throw new ArgumentNullException(nameof(destinationDirectory));
        _fileOperation = OperationFactory.GetCopyOrMoveOperation(logger, file, directory, isMove, isForce);
        _deleteOperation = new DeleteFileOperation(logger, file, directory, isForce);
        _filesFoundResult = new FilesFoundResult(file, directory, destinationDirectory, isForce);
        _fileScanner = fileScanner;
        _directory = directory;
    }

    public bool IsForce => _fileOperation.IsForce;

    public IResult Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        logger.LogInformation($"Starting {nameof(RearrangeByExifRunner)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. {_fileOperation}");

        _fileScanner.FilenameMap.Values
#if RELEASE
        .AsParallel().ForAll(
#else
        .Do(
#endif
        fileDatum =>
        {
            if (fileDatum.Data != null)
            {
                var file = fileDatum.Data.OriginalFilename;
                try
                {
                    var metaDataDirectories = ImageMetadataReader.ReadMetadata(file);

                    var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                    if (possibleDateTime is DateTime dateTime)
                    {
                        // when we can extract a date, there is no error for our usecase
                        _filesFoundResult.Performers.Add(new ToExifPathPerformer(fileDatum, _destinationDirectory, dateTime, _fileOperation));
                        _filesFoundResult.FilesStatistics.FoundImages++;
                        _filesFoundResult.FilesStatistics.FoundXmps += fileDatum.SidecarFiles.Count;
                    }
                    else
                    {
                        // error: we need the date for rearranging
                        _filesFoundResult.NoTimeFoundErrors.Add(new NoTimeFoundError(file, Helpers.GetMetadata(metaDataDirectories)));
                    }

                    var metaDataErrors = metaDataDirectories.SelectMany(t => t.Errors);
                    if (metaDataErrors.Any())
                    {
                        logger.LogTrace("found errors in the metadata while extracting metadata from '{file}': {errors}", file, string.Join(Environment.NewLine, metaDataErrors));
                        _filesFoundResult.MetaDataErrors.Add(new MetaDataError(file, metaDataErrors));
                    }
                }
                catch (MetadataExtractor.ImageProcessingException e)
                {
                    _filesFoundResult.ImageProcessingExceptionErrors.Add(new ImageProcessingExceptionError(file, e));
                }
                catch (Exception e)
                {
                    _filesFoundResult.GeneralExceptionErrors.Add(new GeneralExceptionError(file, e));
                }
            }
        });

        _filesFoundResult.CleanupPerformer.Performer = new DeleteDirectoriesPerformer(_sourceDirectory, _deleteOperation);
        logger.LogInformation($"{nameof(RearrangeByExifRunner)}.{nameof(Run)} has finished");

        return _filesFoundResult;
    }
}