using Microsoft.Extensions.Logging;


using SortPhotosWithXmp.Result;

namespace SortPhotosWithXmp.Features;

public interface IRun
{
    public IResult Run(ILogger logger);
    public bool IsForce { get; }
}
