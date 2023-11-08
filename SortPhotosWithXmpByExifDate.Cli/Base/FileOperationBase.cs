using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public abstract class FileOperationBase : IOperation
    {
        private readonly IDirectory _directory;

        protected FileOperationBase(IDirectory directory, bool force)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            Force = force;
        }

        public bool Force { get; private set; }

        public void CreateDirectory(string targetPath)
        {
            // make sure we are not working on a file
            targetPath = Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException($"Path.GetDirectoryName({targetPath})");

            if (Force && !_directory.Exists(targetPath))
            {
                _ = _directory.CreateDirectory(targetPath);
            }
        }

        public abstract void ChangeFiles(IEnumerable<IImageFile> files, string targetPath);
    }
}