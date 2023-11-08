using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.Features.CheckIfFileNameContainsDateDifferentToExifDates;

internal class CheckIfFileNameContainsDateDifferentToExifDatesRunner : IRun
{
    public CheckIfFileNameContainsDateDifferentToExifDatesRunner(string source, bool force) => throw new NotImplementedException();

    public bool Force { get; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}
