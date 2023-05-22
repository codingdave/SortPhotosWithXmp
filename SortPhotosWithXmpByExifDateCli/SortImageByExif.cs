using System.Diagnostics;
using MetadataExtractor;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal class SortImageByExif : IRun
{
    private readonly DirectoryInfo _destinationDirectory;
    private readonly DirectoryInfo _sourceDirectory;
    private readonly IEnumerable<string> _extensions;
    private readonly ImagesAndXmpFoundStatistics _statistics;
    private readonly bool _force;
    private readonly bool _move;

    internal SortImageByExif(DirectoryInfo sourceDirectoryInfo, DirectoryInfo destinationDirectoryInfo, IEnumerable<string> extensions, bool force, bool move)
    {
        _sourceDirectory = sourceDirectoryInfo ?? throw new ArgumentNullException(nameof(sourceDirectoryInfo));
        _destinationDirectory = destinationDirectoryInfo ?? throw new ArgumentNullException(nameof(destinationDirectoryInfo));
        _extensions = extensions;
        _force = force;
        _move = move;
        _statistics = new ImagesAndXmpFoundStatistics(force, move);
    }

    private IEnumerable<FileInfo> GetFileInfos() =>
        _sourceDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(f => _extensions.Any(e => f.Name.EndsWith(e, StringComparison.OrdinalIgnoreCase)));

    public IStatistics Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        var operation = _move ? "move" : "copy";
        logger.LogInformation($"Starting {nameof(SortPhotosWithXmpByExifDateCli)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. force: {_force}, operation: {operation}");

        foreach (var fileInfo in GetFileInfos())
        {
            var metaDataDirectories = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
            var errorLogged = LogError(fileInfo, metaDataDirectories);

            var dateTime = dateTimeResolver.GetDateTimeFromImage(metaDataDirectories);
            if (dateTime != DateTime.MinValue)
            {
                var xmpFiles = Helpers.GetCorrespondingXmpFiles(fileInfo);
                Helpers.MoveImageAndXmpToExifPath(logger, fileInfo, xmpFiles, dateTime, _destinationDirectory, _statistics, _force, _move);
            }
            else
            {
                var errorMessage = new List<string>() { "No time found." };
                errorMessage.AddRange(Helpers.GetMetadata(metaDataDirectories));
                _statistics.FileError.Add(Helpers.GetError(fileInfo, errorMessage));
            }
        }

        var statistics = new DirectoriesDeletedStatistics(_force);
        Helpers.RecursivelyDeleteEmptyDirectories(_sourceDirectory, statistics, _force);
        return new ImagesAndXmpCopiedDirectoriesDeletedStatistics(_statistics, statistics);

        bool LogError(FileInfo fileInfo, IReadOnlyList<MetadataExtractor.Directory> metaDataDirectories)
        {
            var errors = Helpers.GetError(fileInfo, metaDataDirectories);
            if (errors.HasErrors)
            {
                _statistics.FileError.Add(errors);
            }

            return errors.HasErrors;
        }
    }
}
