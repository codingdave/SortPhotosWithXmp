using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class FilesStatistics : IFilesStatistics
{
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }
    public int SkippedXmps { get; set; }
    public int SkippedImages { get; set; }

    public void Log(ILogger logger)
    {
        logger.LogInformation("-> Found images: {FoundImages}, xmps: {FoundXmps} and duplicates {SkippedImages}/{SkippedXmps}.", FoundImages, FoundXmps, SkippedImages, SkippedXmps);
    }
}
