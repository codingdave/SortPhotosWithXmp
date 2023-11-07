using System.Collections.Concurrent;

using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class SuccessCollection : ISuccessCollection
{
    public IReadOnlyList<ISuccess> Successes => _successes.ToList();
    private readonly ConcurrentBag<ISuccess> _successes = new();

    public void Add(ISuccess success)
    {
        _successes.Add(success);
    }
}
