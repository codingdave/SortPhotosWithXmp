using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Features;

public interface IRun
{
    public IResult Run(ILogger logger);
    public bool IsForce { get; }
}
