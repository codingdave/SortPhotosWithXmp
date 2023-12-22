using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Performer;

namespace SortPhotosWithXmpByExifDate.Result;

public interface IResult
{
    void Log(ILogger logger);
    IPerformerCollection Performers { get; }
}