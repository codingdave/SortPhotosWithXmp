using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Extensions;

namespace SortPhotosWithXmp.Performer;

public class PerformerCollection : IPerformerCollection
{
    public IReadOnlyList<IPerformer> Performers => _performers.ToList();
    private readonly ConcurrentBag<IPerformer> _performers = new();

    public void Add(IPerformer performer)
    {
        _performers.Add(performer);
    }

    public void Perform(ILogger logger)
    {
        if (Performers.Any())
        {
            logger.LogInformation($"Performing {Performers.Count} successful operations");
            Performers.Do(performer => performer.Perform(logger));
        }
    }
}
