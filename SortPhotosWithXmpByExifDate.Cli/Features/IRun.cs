using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Result;

namespace SortPhotosWithXmpByExifDate.Features;

public interface IRun
{
    public IResult Run(ILogger logger);
    public bool IsForce { get; }
}
