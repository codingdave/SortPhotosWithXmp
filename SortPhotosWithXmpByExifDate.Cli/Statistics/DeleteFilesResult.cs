using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Performer;

namespace SortPhotosWithXmpByExifDate.Result;

public class DeleteFilesResult : IResult
{
    public IPerformerCollection Performers => throw new NotImplementedException();

    public void Log(ILogger logger)
    {
        throw new NotImplementedException();
    }
}
