using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public interface IStatistics
{
    void Log();
    IReadOnlyErrorCollection FileErrors { get; }
}