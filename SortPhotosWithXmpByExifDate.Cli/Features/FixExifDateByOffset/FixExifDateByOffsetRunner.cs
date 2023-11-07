using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.Features.FixExifDateByOffset;

internal class FixExifDateByOffsetRunner : IRun
{
    public FixExifDateByOffsetRunner(string dir, TimeSpan offset, bool force) => throw new NotImplementedException();

    public bool Force { get; set; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}