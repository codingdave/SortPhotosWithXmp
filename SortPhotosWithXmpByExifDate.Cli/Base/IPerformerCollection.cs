using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli
{
    public interface IPerformerCollection : IReadOnlyPerformerCollection
    {
        void Add(IPerformer performer);
    }
}