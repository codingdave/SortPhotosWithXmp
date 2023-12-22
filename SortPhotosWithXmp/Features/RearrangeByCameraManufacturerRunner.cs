using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Result;

namespace SortPhotosWithXmp.Features;

public class RearrangeByCameraManufacturerRunner : IRun
{
    public RearrangeByCameraManufacturerRunner(string source, string destination, bool isForce)
    {
        throw new NotImplementedException();
    }

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}