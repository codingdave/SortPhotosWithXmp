using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Performer;

namespace SortPhotosWithXmpByExifDate.Result;

internal class DuplicatesDeletedResult : IResult
{
    public IPerformerCollection Performers => throw new NotImplementedException();

    public void Log(ILogger logger)
    {
        throw new NotImplementedException();
    }
}