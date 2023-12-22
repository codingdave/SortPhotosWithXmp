using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Features.FixExifDateByOffset;

internal class FixExifDateByOffsetRunner : IRun
{
    public FixExifDateByOffsetRunner(string dir, TimeSpan offset, bool isForce) => throw new NotImplementedException();

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}