using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IStatistics
{
    void Log();
    IReadOnlyErrorCollection FileErrors { get; }
}