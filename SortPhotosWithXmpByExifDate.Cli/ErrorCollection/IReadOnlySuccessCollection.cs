using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface IReadOnlySuccessCollection
    {
        public IReadOnlyList<ISuccess> Successes { get; }
    }
}