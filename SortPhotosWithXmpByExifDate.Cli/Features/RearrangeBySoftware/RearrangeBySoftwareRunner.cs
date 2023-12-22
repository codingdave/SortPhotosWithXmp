using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Result;

namespace SortPhotosWithXmpByExifDate.Features.RearrangeBySoftware;

internal class RearrangeBySoftwareRunner : IRun
{
    public RearrangeBySoftwareRunner(string source, string destination, bool isForce) => throw new NotImplementedException();

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}