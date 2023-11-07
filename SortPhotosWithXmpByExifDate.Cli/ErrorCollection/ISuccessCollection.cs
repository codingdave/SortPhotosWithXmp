using SortPhotosWithXmpByExifDate.Cli.Statistics;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface ISuccessCollection : IReadOnlySuccessCollection
    {
        void Add(ISuccess success);
    }
}