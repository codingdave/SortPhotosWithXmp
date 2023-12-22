using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Performer;

namespace SortPhotosWithXmp.Result;

public class DeleteFilesResult : IResult
{
    public IPerformerCollection Performers => throw new NotImplementedException();

    public void Log(ILogger logger)
    {
        throw new NotImplementedException();
    }
}
