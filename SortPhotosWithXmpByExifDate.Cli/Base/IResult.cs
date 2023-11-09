using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public interface IResult
{
    void Log(ILogger logger);
    IPerformerCollection PerformerCollection { get; }
}