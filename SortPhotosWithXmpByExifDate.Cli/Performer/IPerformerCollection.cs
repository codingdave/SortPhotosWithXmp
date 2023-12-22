using Microsoft.Extensions.Logging;


namespace SortPhotosWithXmpByExifDate.Performer;

public interface IPerformerCollection
{
    public IReadOnlyList<IPerformer> Performers { get; }
    void Add(IPerformer performer);
    void Perform(ILogger logger);
}