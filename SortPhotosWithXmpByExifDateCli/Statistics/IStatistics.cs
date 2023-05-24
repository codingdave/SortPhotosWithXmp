using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IStatistics
{
    void Log();
    IReadOnlyErrorCollection FileErrors { get; }
}