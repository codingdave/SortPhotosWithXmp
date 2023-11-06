using MetadataExtractor;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByExif;

internal class RearrangeByExifRunner : IRun
{
    private readonly string _destinationDirectory;
    private readonly string _sourceDirectory;
    private readonly FilesFoundStatistics _statistics;
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
        _operationPerformer = OperationPerformerFactory.GetCopyOrMovePerformer(logger, file, move, force);
        _statistics = new FilesFoundStatistics(logger, _operationPerformer);
        _fileScanner = fileScanner;
        _directory = directory;
        _deleteDirectoryOperation = new DeleteDirectoryOperation(logger, directory, force);
    }

    public IStatistics Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        logger.LogInformation($"Starting {nameof(RearrangeByExifRunner)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. {_operationPerformer}");

        foreach (var fileDatum in _fileScanner.Map.Values)
        {
            if (fileDatum.Data != null)
            {
                var file = fileDatum.Data.OriginalFilename;
                try
                {
                    var metaDataDirectories = ImageMetadataReader.ReadMetadata(file);
                    var errors = Helpers.GetErrorsFromMetadata(metaDataDirectories);
                    if (errors.Any())
                    {
                        logger.LogTrace("found errors while extracting metadata from '{file}'", file);
                        _statistics.AddError(new MetaDataError(file, errors));
                    }

                    var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                    if (possibleDateTime is DateTime dateTime)
                    {
                        logger.LogTrace("Extracted date {dateTime} from '{file}'", dateTime, file);

                        if (!errors.Any())
                        {
                            Helpers.MoveImageAndXmpToExifPath(_directory, _fileScanner.Map[file], dateTime, _destinationDirectory, _statistics, _operationPerformer);
                        }
                        else
                        {
                            logger.LogTrace("Keep '{file}' as errors have happened. We will copy it later when dealing about the error.", file);
                        }
                    }
                    else
                    {
                        _statistics.AddError(new NoTimeFoundError(file, Helpers.GetMetadata(metaDataDirectories)));
                    }
                }
                catch (MetadataExtractor.ImageProcessingException e)
                {
                    _statistics.AddError(new ImageProcessingExceptionError(file, e));
                }
                catch (Exception e)
                {
                    logger.LogExceptionError($"Failed processing {file}:", e);
                    _statistics.AddError(new ExceptionError(file, e));
                }
            }
        }

        var statistics = new DirectoriesDeletedStatistics(logger, _deleteDirectoryOperation);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _sourceDirectory, _deleteDirectoryOperation);
        return new FilesAndDirectoriesStatistics(_statistics, statistics);
    }
}