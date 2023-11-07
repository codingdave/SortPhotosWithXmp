using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.Statistics;

namespace SortPhotosWithXmpByExifDate.Cli;

public interface IRun
{
    public IStatistics Run(ILogger logger);
}
