using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class FilesStatistics : IFilesStatistics
{
    private readonly ILogger _logger;

    public FilesStatistics(ILogger logger) => _logger = logger;

    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }
    public int SkippedXmps { get; set; }
    public int SkippedImages { get; set; }

    public void Log()
    {
        _logger.LogInformation("-> Found images: {FoundImages}, xmps: {FoundXmps} and duplicates {SkippedImages}/{SkippedXmps}.", FoundImages, FoundXmps, SkippedImages, SkippedXmps);
    }
}
