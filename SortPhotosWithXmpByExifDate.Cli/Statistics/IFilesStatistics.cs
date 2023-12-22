using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Statistics;

public interface IFilesStatistics
{
    int FoundXmps { get; set; }
    int FoundImages { get; set; }
    int SkippedXmps { get; set; }
    int SkippedImages { get; set; }

    void Log(ILogger logger);
}
