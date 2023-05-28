using MetadataExtractor;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal class SortImageByExif : IRun
{
    private readonly string _destinationDirectory;
    private readonly string _sourceDirectory;
    private readonly IEnumerable<string> _extensions;
    private readonly FilesFoundStatistics _statistics;
    private readonly IFileOperation _operationPerformer;
    private readonly DeleteDirectoryOperation _deleteDirectoryOperation;

    internal SortImageByExif(ILogger logger,
                             string sourceDirectory,
                             string destinationDirectory,
                             IEnumerable<string> extensions,
                             IFileOperation operationPerformer,
                             DeleteDirectoryOperation deleteDirectoryPerformer)
    {
        _sourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
        _destinationDirectory = destinationDirectory ?? throw new ArgumentNullException(nameof(destinationDirectory));
        _extensions = extensions;
        _statistics = new FilesFoundStatistics(logger, operationPerformer);
        _operationPerformer = operationPerformer;
        _deleteDirectoryOperation = deleteDirectoryPerformer;
    }

    private IEnumerable<string> GetFileInfos() =>
        System.IO.Directory.EnumerateFiles(_sourceDirectory, "*.*", SearchOption.AllDirectories)
            .Where(f => _extensions.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)));

    public IStatistics Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        logger.LogInformation($"Starting {nameof(SortPhotosWithXmpByExifDateCli)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. {_operationPerformer}");

        foreach (var file in GetFileInfos())
        {
            try
            {
                var metaDataDirectories = ImageMetadataReader.ReadMetadata(file);
                var errors = Helpers.GetErrorsFromMetadata(metaDataDirectories);
                if (errors.Any())
                {
                    _statistics.AddError(new MetaDataError(file, errors));
                }

                var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                if (possibleDateTime is DateTime dateTime)
                {
                    logger.LogTrace("Extracted date {date} from {file}", dateTime, file);
                    var xmpFiles = Helpers.GetCorrespondingXmpFiles(file);
                    Helpers.MoveImageAndXmpToExifPath(logger, file, xmpFiles, dateTime, _destinationDirectory, _statistics, _operationPerformer);
                }
                else
                {
                    _statistics.AddError(new NoTimeFoundError(file, Helpers.GetMetadata(metaDataDirectories)));
                }
            }
            catch (ImageProcessingException e)
            {
                _statistics.AddError(new ImageProcessingExceptionError(file, e));
            }
            catch (Exception e)
            {
                logger.LogExceptionError($"Failed processing {file}:", e);
                _statistics.AddError(new ExceptionError(file, e));
            }
        }

        var statistics = new DirectoriesDeletedStatistics(logger, _deleteDirectoryOperation);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _sourceDirectory, _deleteDirectoryOperation);
        return new FilesAndDirectoriesStatistics(_statistics, statistics);
    }
}
