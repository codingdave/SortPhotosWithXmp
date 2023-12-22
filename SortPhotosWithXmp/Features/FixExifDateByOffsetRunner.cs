using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Result;

namespace SortPhotosWithXmp.Features;

public class FixExifDateByOffsetRunner : IRun
{
    public FixExifDateByOffsetRunner(string dir, TimeSpan offset, bool isForce) => throw new NotImplementedException();

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}