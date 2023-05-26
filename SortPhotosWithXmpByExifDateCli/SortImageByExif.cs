using MetadataExtractor;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal class SortImageByExif : IRun
{
    private readonly DirectoryInfo _destinationDirectory;
    private readonly DirectoryInfo _sourceDirectory;
    private readonly IEnumerable<string> _extensions;
    private readonly FilesFoundStatistics _statistics;
    private readonly IFileOperation _operationPerformer;
    private readonly DeleteDirectoryOperation _deleteDirectoryPerformer;

    internal SortImageByExif(ILogger logger,
                             DirectoryInfo sourceDirectoryInfo,
                             DirectoryInfo destinationDirectoryInfo,
                             IEnumerable<string> extensions,
                             IFileOperation operationPerformer,
                             DeleteDirectoryOperation deleteDirectoryPerformer)
    {
        _sourceDirectory = sourceDirectoryInfo ?? throw new ArgumentNullException(nameof(sourceDirectoryInfo));
        _destinationDirectory = destinationDirectoryInfo ?? throw new ArgumentNullException(nameof(destinationDirectoryInfo));
        _extensions = extensions;
        _statistics = new FilesFoundStatistics(logger, operationPerformer);
        _operationPerformer = operationPerformer;
        _deleteDirectoryPerformer = deleteDirectoryPerformer;
    }

    private IEnumerable<FileInfo> GetFileInfos() =>
        _sourceDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(f => _extensions.Any(e => f.Name.EndsWith(e, StringComparison.OrdinalIgnoreCase)));

    public IStatistics Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        logger.LogInformation($"Starting {nameof(SortPhotosWithXmpByExifDateCli)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. {_operationPerformer}");

        foreach (var fileInfo in GetFileInfos())
        {
            try
            {
                var metaDataDirectories = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
                var errors = Helpers.GetErrorsFromMetadata(metaDataDirectories);
                if (errors.Any())
                {
                    _statistics.AddError(new MetaDataError(fileInfo, errors));
                }

                var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                if (possibleDateTime is DateTime dateTime)
                {
                    logger.LogTrace("Extracted date {date} from {file}", dateTime, fileInfo.FullName);
                    var xmpFiles = Helpers.GetCorrespondingXmpFiles(fileInfo);
                    Helpers.MoveImageAndXmpToExifPath(logger, fileInfo, xmpFiles, dateTime, _destinationDirectory, _statistics, _operationPerformer);
                }
                else
                {
                    _statistics.AddError(new NoTimeFoundError(fileInfo, Helpers.GetMetadata(metaDataDirectories)));
                }
            }
            catch (ImageProcessingException e)
            {
                _statistics.AddError(new ImageProcessingExceptionError(fileInfo, e));
            }
            catch (Exception e)
            {
                logger.LogExceptionError($"Failed processing {fileInfo}:", e);
                _statistics.AddError(new ExceptionError(fileInfo, e));
            }
        }

        var statistics = new DirectoriesDeletedStatistics(logger, _deleteDirectoryPerformer);
        Helpers.RecursivelyDeleteEmptyDirectories(_sourceDirectory, statistics, _deleteDirectoryPerformer);
        return new FilesAndDirectoriesStatistics(_statistics, statistics);
    }
}
