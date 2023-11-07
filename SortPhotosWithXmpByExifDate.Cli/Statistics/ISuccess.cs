using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public interface ISuccess
{
    void Perform(ILogger logger);
}