using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.Statistics;

namespace SortPhotosWithXmpByExifDate.Cli;

internal interface IRun
{
    public IStatistics Run(ILogger logger);
}
