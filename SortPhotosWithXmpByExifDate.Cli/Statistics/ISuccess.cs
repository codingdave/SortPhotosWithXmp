using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public interface ISuccess
{
    void Perform(ILogger logger);
}