using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IStatistics
{
    void Log(ILogger logger);
    IReadOnlyFileError ReadOnlyFileError { get; }
}