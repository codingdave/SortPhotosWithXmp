using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal interface IRunAsync
{
    public Task<IStatistics> RunAsync(ILogger logger);
}