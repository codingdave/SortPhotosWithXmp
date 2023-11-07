using MetadataExtractor;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Extensions;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByExif;

internal class RearrangeByExifRunner : IRun
{
    private readonly string _destinationDirectory;
    private readonly string _sourceDirectory;
    private readonly FilesFoundResult _filesFoundStatistics;
    private readonly IFileOperation _operationPerformer;
    private readonly IFileScanner _fileScanner;
    private readonly IDirectory _directory;

    private readonly DeleteDirectoryOperation _deleteDirectoryOperation;

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
        _filesFoundStatistics = new FilesFoundResult(logger, _operationPerformer);
        _fileScanner = fileScanner;
        _directory = directory;
        _deleteDirectoryOperation = new DeleteDirectoryOperation(logger, directory, force);
    }

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
                        _filesFoundStatistics.AddError(new MetaDataError(file, errors));
                    }

                    var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                    if (possibleDateTime is not DateTime dateTime)
                    {
                        hasErrors = true;
                        _filesFoundStatistics.AddError(new NoTimeFoundError(file, Helpers.GetMetadata(metaDataDirectories)));
                    }
                    else
                    {
                        _filesFoundStatistics.AddSuccessful(new ToExifPath(fileDatum, _destinationDirectory, dateTime, _operationPerformer));
                        _filesFoundStatistics.FoundImages++;
                        _filesFoundStatistics.FoundXmps += fileDatum.SidecarFiles.Count;
                    }

                    if (hasErrors)
                    {
                        logger.LogTrace("Keep '{file}' as errors have happened. We will copy it later when dealing about the error.", file);
                    }
                }
                catch (MetadataExtractor.ImageProcessingException e)
                {
                    _filesFoundStatistics.AddError(new ImageProcessingExceptionError(file, e));
                }
                catch (Exception e)
                {
                    _filesFoundStatistics.AddError(new GeneralExceptionError(file, e));
                }
            }
        });

#warning Delete in then end, add a Cleanup/final step that comes after processing successful and unsuccessful data. This will also simplify the Merging of FilesAndDirectoriesResult
        // _filesFoundStatistics.AddCleanup();
        logger.LogInformation($"{nameof(RearrangeByExifRunner)}.{nameof(Run)} has finished");
        var directoriesDeletedStatistics = new DirectoriesDeletedResult(logger, _deleteDirectoryOperation);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _directory, _sourceDirectory, _deleteDirectoryOperation);
        return new FilesAndDirectoriesResult(_filesFoundStatistics, directoriesDeletedStatistics);
    }
}