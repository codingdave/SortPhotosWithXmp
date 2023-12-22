using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Result;

namespace SortPhotosWithXmp.Features;

public class RearrangeBySoftwareRunner : IRun
{
    public RearrangeBySoftwareRunner(string source, string destination, bool isForce) => throw new NotImplementedException();

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}