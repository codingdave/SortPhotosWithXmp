using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Repository;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public interface ICopyOrMoveFileOperation : IOperation
    {
        void ChangeFiles(IEnumerable<IImageFile> files, string targetPath);
    }
}