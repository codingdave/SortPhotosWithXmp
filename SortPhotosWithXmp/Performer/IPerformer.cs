using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmp.Performer;

public interface IPerformer
{
    void Perform(ILogger logger);
}