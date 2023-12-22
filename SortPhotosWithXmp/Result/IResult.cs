using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Performer;

namespace SortPhotosWithXmp.Result;

public interface IResult
{
    void Log(ILogger logger);
    IPerformerCollection Performers { get; }
}