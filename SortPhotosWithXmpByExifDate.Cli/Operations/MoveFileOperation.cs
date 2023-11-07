using ImageMagick;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class MoveFileOperation : ICopyOrMoveFileOperation
    {
        private readonly ILogger _logger;
        private readonly IFile _file;
        private readonly IDirectory _directory;

        internal MoveFileOperation(ILogger logger, IFile file, IDirectory directory, bool force)
        {
            _logger = logger;
            _file = file;
            _directory = directory;
            Force = force;
        }

        public bool Force { get; }

        public void ChangeFiles(IEnumerable<IImageFile> files, string targetPath)
        {
            if (Force && !_directory.Exists(targetPath))
            {
                _ = _directory.CreateDirectory(targetPath);
            }

            foreach (var file in files)
            {
                var targetName = Path.Combine(targetPath, Path.GetFileName(file.CurrentFilename));

                if (File.Exists(targetName))
                {
                    var error = new FileAlreadyExistsError(targetName, file.CurrentFilename, $"File {file.CurrentFilename} already exists at {targetName}");
                    _logger.LogError(error.ToString());
                }
                else
                {
                    if (Force)
                    {
                        _logger.LogTrace($"IFile.Move({file.CurrentFilename}, {targetName});");
                        _file.Move(file.CurrentFilename, targetName);
                    }
                    else
                    {
                        _logger.LogTrace($"Ignoring IFile.Move({file.CurrentFilename}, {targetName});");
                    }
                    file.NewFilename = targetName;
                }
            }
        }

        public override string ToString()
        {
            var message = Force ? "performing" : "simulating";
            message += " move";
            return message;
        }
    }
}