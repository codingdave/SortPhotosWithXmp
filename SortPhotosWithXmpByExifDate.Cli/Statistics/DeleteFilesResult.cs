using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class DeleteFilesResult : IResult
{
    public IPerformerCollection Performers => throw new NotImplementedException();

    public void Log(ILogger logger)
    {
        throw new NotImplementedException();
    }
}
