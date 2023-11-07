using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByCameraManufacturer;

internal class RearrangeByCameraManufacturerRunner : IRun
{
    public RearrangeByCameraManufacturerRunner(string source, string destination, bool force)
    {
        throw new NotImplementedException();
    }

    public bool Force { get; set; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}