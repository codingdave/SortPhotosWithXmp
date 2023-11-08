using System.Collections.Concurrent;

using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class PerformerCollection : IPerformerCollection
{
    public IReadOnlyList<IPerformer> Performers => _performers.ToList();
    private readonly ConcurrentBag<IPerformer> _performers = new();

    public void Add(IPerformer performer)
    {
        _performers.Add(performer);
    }
}
