using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Repository;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public interface IFileOperation : IOperation
    {
        void ChangeFiles(IEnumerable<IImageFile> files, string targetPath);
    }
}