using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal interface IRun
{
    public IStatistics Run(ILogger logger);
}
