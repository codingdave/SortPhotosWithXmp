using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface ISuccessCollection : IReadOnlySuccessCollection
    {
        void Add(ISuccess success);
    }
}