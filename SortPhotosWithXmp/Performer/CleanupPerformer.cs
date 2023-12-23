using Microsoft.Extensions.Logging;


namespace SortPhotosWithXmp.Performer;

public class CleanupPerformer : IPerformer
{
    public DeleteDirectoriesPerformer Performer { get; }

    public CleanupPerformer(DeleteDirectoriesPerformer performer) => Performer = performer;

    public void Perform(ILogger logger)
    {
        if (Performer != null)
        {
            logger.LogInformation("Performing Cleanup.");
        }
    }
}