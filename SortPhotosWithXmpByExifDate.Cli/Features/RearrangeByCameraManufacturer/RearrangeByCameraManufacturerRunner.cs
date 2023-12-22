using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Features.RearrangeByCameraManufacturer;

internal class RearrangeByCameraManufacturerRunner : IRun
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