using MetadataExtractor;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;
using SortPhotosWithXmpByExifDateCli.Operation;
using SortPhotosWithXmpByExifDateCli.Repository;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli.Runners.SortImageByExif;

internal class SortImagesByExifRunner : IRun
{
    private readonly string _destinationDirectory;
    private readonly string _sourceDirectory;
    private readonly FilesFoundStatistics _statistics;
    private readonly IFileOperation _operationPerformer;
    private readonly FileScanner _fileScanner;
    private readonly DeleteDirectoryOperation _deleteDirectoryOperation;

    internal SortImagesByExifRunner(ILogger logger,
                             string sourceDirectory,
                             string destinationDirectory,
                             FileScanner fileScanner,
                             IFileOperation operationPerformer,
                             DeleteDirectoryOperation deleteDirectoryPerformer)
    {
        _sourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
        _destinationDirectory = destinationDirectory ?? throw new ArgumentNullException(nameof(destinationDirectory));
        _statistics = new FilesFoundStatistics(logger, operationPerformer);
        _operationPerformer = operationPerformer;
        _fileScanner = fileScanner;
        _deleteDirectoryOperation = deleteDirectoryPerformer;
    }

    public IStatistics Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        logger.LogInformation($"Starting {nameof(SortImagesByExifRunner)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. {_operationPerformer}");

        foreach (var fileDatum in _fileScanner.All)
        {
            if (fileDatum.Data != null)
            {
                var file = fileDatum.Data.Filename;
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
                        var xmpFiles = Helpers.GetCorrespondingXmpFiles(file);
                        if (!errors.Any())
                        {
                            Helpers.MoveImageAndXmpToExifPath(file, xmpFiles, dateTime, _destinationDirectory, _statistics, _operationPerformer);
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
