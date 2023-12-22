using Microsoft.Extensions.Logging;


namespace SortPhotosWithXmpByExifDate.Performer;

public class CleanupPerformer : IPerformer
{
    public DeleteDirectoriesPerformer Performer { get; internal set; }

    public void Perform(ILogger logger)
    {
        if (Performer != null)
        {
            logger.LogInformation("Performing Cleanup.");
        }
    }
}