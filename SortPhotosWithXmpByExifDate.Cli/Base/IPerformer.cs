using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public interface IPerformer
{
    void Perform(ILogger logger);
}