using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public interface IResult
{
    void Log(ILogger logger);
    IPerformerCollection PerformerCollection { get; }
}