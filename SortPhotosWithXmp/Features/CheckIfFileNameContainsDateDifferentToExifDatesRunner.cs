using Microsoft.Extensions.Logging;


using SortPhotosWithXmp.Result;

namespace SortPhotosWithXmp.Features;

public class CheckIfFileNameContainsDateDifferentToExifDatesRunner : IRun
{
    public CheckIfFileNameContainsDateDifferentToExifDatesRunner(string source, bool isForce) => throw new NotImplementedException();

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
        throw new NotImplementedException();
    }
}
