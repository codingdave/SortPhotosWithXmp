using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IStatistics
{
    void Log();
    IReadOnlyErrorCollection FileErrors { get; }
}