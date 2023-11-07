using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SortPhotosWithXmpByExifDate.Cli.Repository;


using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class CopyFileOperation : ICopyOrMoveFileOperation
    {
        private readonly ILogger _logger;
        private readonly IFile _file;
        private readonly IDirectory _directory;

        internal CopyFileOperation(
            ILogger logger,
            IFile file,
            IDirectory directory,
            bool force)
        {
            _logger = logger;
            _file = file;
            _directory = directory;
            Force = force;
        }

        public bool Force { get; }

        private void ChangeFile(string sourceFileName, string destFileName)
        {
            if (Force)
            {
                _logger.LogTrace($"IFile.Copy({sourceFileName}, {destFileName});");
                _file.Copy(sourceFileName, destFileName);
            }
            else
            {
                _logger.LogTrace($"Ignoring IFile.Copy({sourceFileName}, {destFileName});");
            }
        }

        public void ChangeFiles(IEnumerable<IImageFile> files, string targetPath)
        {
            if (!_directory.Exists(targetPath))
            {
                _ = _directory.CreateDirectory(targetPath);
            }

            foreach (var file in files)
            {
                ChangeFile(file.CurrentFilename, targetPath);
            }
        }

        public override string ToString()
        {
            var message = Force ? "performing" : "simulating";
            message += " copy";
            return message;
        }
    }
}