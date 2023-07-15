using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Entities;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal class DeleteLonelyXmps : IRun
{
    private bool _force;
    private FileScanner _fileScanner;

    public DeleteLonelyXmps(bool force, FileScanner fileScanner)
    {
        _force = force;
        _fileScanner = fileScanner;
    }

    public IStatistics Run(ILogger logger)
    {
        // find all xmps that do not have an image
        var lonelies = _fileScanner.LonelySidecarFiles;
        logger.LogInformation($"Found lonely xmps: {string.Join(", ", lonelies)}");
        if (_force)
        {
            foreach (var lonely in lonelies)
            {
                File.Delete(lonely);
            }
        }
        #warning implement statistics
        return null;
    }
}