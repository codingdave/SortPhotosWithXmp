using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli
{
    public interface IReadOnlyPerformerCollection
    {
        public IReadOnlyList<IPerformer> Performers { get; }
    }
}