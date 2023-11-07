using SortPhotosWithXmpByExifDate.Cli.Statistics;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface IReadOnlySuccessCollection
    {
        public IReadOnlyList<ISuccess> Successes { get; }
    }
}