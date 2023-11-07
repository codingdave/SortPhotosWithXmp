using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli;

public interface IRun
{
    public IResult Run(ILogger logger);
    public bool Force { get; set; }
}
