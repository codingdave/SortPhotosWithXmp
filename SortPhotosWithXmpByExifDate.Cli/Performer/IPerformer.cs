using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Performer;

public interface IPerformer
{
    void Perform(ILogger logger);
}