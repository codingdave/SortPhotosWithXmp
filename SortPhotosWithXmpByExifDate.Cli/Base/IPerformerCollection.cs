using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli
{
    public interface IPerformerCollection
    {
        public IReadOnlyList<IPerformer> Performers { get; }
        void Add(IPerformer performer);
    }
}