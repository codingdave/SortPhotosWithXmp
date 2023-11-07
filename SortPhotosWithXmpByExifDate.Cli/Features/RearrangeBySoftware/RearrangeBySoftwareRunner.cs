using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.Features.RearrangeBySoftware;

internal class RearrangeBySoftwareRunner : IRun
{
    public RearrangeBySoftwareRunner(string source, string destination, bool force) => throw new NotImplementedException();

    public bool Force { get; set; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}