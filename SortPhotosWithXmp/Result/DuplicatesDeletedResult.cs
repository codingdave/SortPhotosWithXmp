using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Performer;

namespace SortPhotosWithXmp.Result;

internal class DuplicatesDeletedResult : IResult
{
    public IPerformerCollection Performers => throw new NotImplementedException();

    public void Log(ILogger logger)
    {
        throw new NotImplementedException();
    }
}