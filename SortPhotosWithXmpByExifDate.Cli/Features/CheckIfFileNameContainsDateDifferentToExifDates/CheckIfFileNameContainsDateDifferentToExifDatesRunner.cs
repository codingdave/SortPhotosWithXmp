using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Features.CheckIfFileNameContainsDateDifferentToExifDates;

internal class CheckIfFileNameContainsDateDifferentToExifDatesRunner : IRun
{
    public CheckIfFileNameContainsDateDifferentToExifDatesRunner(string source, bool isForce) => throw new NotImplementedException();

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}
