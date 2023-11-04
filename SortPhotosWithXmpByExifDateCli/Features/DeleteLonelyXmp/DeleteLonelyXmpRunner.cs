using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Repository;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli.Features.DeleteLonelyXmp;

internal class DeleteLonelyXmpRunner : IRun
{
    private readonly bool _force;
    private readonly IFileScanner _fileScanner;

    public DeleteLonelyXmpRunner(bool force, IFileScanner fileScanner)
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
                File.Delete(lonely.Filename);
            }
        }

        return new DeleteFilesStatistics();
    }
}
