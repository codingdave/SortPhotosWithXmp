using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class DeleteFilesResult : IResult
{
    public IPerformerCollection PerformerCollection => throw new NotImplementedException();

    public void Log(ILogger logger)
    {
        throw new NotImplementedException();
    }
}
