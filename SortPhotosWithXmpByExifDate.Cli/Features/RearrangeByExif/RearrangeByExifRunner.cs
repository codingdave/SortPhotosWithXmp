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
    private readonly FileOperationBase _operationPerformer;
    private readonly IFileScanner _fileScanner;
    private readonly IDirectory _directory;

    internal RearrangeByExifRunner(ILogger logger,
                             string sourceDirectory,
                             string destinationDirectory,
                             IFileScanner fileScanner,
                             IFile file,
                             IDirectory directory,
                             bool move,
                             bool force)
    {
        _sourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
        _destinationDirectory = destinationDirectory ?? throw new ArgumentNullException(nameof(destinationDirectory));
        _operationPerformer = OperationPerformerFactory.GetCopyOrMovePerformer(logger, file, directory, move, force);
        _filesFoundResult = new FilesFoundResult(destinationDirectory);
        _fileScanner = fileScanner;
        _directory = directory;
    }

    public bool Force => _operationPerformer.Force;

    public IResult Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        logger.LogInformation($"Starting {nameof(RearrangeByExifRunner)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. {_operationPerformer}");

        _fileScanner.Map.Values
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
                    var errors = metaDataDirectories.SelectMany(t => t.Errors);
                    var hasErrors = errors.Any();
                    if (hasErrors)
                    {
                        logger.LogTrace("found errors while extracting metadata from '{file}': {errors}", file, string.Join(Environment.NewLine, errors));
                        _filesFoundResult.AddError(new MetaDataError(file, errors));
                    }

                    var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                    if (possibleDateTime is not DateTime dateTime)
                    {
                        hasErrors = true;
                        _filesFoundResult.AddError(new NoTimeFoundError(file, Helpers.GetMetadata(metaDataDirectories)));
                    }
                    else
                    {
                        _filesFoundResult.AddPerformer(new ToExifPathPerformer(fileDatum, _destinationDirectory, dateTime, _operationPerformer));
                        _filesFoundResult.FilesStatistics.FoundImages++;
                        _filesFoundResult.FilesStatistics.FoundXmps += fileDatum.SidecarFiles.Count;
                    }

                    if (hasErrors)
                    {
                        logger.LogTrace("Keep '{file}' as errors have happened. We will copy it later when dealing about the error.", file);
                    }
                }
                catch (MetadataExtractor.ImageProcessingException e)
                {
                    _filesFoundResult.AddError(new ImageProcessingExceptionError(file, e));
                }
                catch (Exception e)
                {
                    _filesFoundResult.AddError(new GeneralExceptionError(file, e));
                }
            }
        });

#warning Result or Task List?
        _filesFoundResult.CleanupResult = new DirectoriesDeletedResult(_directory, _sourceDirectory);
        logger.LogInformation($"{nameof(RearrangeByExifRunner)}.{nameof(Run)} has finished");

        return _filesFoundResult;
    }
}