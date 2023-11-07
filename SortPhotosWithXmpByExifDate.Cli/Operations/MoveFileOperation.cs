using ImageMagick;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class MoveFileOperation : IFileOperation
    {
        private readonly ILogger _logger;
        private readonly IFile _file;
        private readonly IDirectory _directory;


        internal MoveFileOperation(ILogger logger, IFile file, IDirectory directory, bool force)
        {
            _logger = logger;
            _file = file;
            _directory = directory;

            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void ChangeFiles(IEnumerable<IImageFile> files, string targetPath)
        {
            if (IsChanging && !_directory.Exists(targetPath))
            {
                _ = _directory.CreateDirectory(targetPath);
            }

            foreach (var file in files)
            {
                var targetName = Path.Combine(targetPath, Path.GetFileName(file.OriginalFilename));

                if (File.Exists(targetName))
                {
                    var error = new FileAlreadyExistsError(targetName, file.OriginalFilename, $"File {file.OriginalFilename} already exists at {targetName}");
                    _logger.LogError(error.ToString());
                }
                else
                {
                    if (IsChanging)
                    {
                        _logger.LogTrace($"IFile.Move({file.OriginalFilename}, {targetName});");
                        _file.Move(file.OriginalFilename, targetName);
                        file.NewFilename = targetName;
                    }
                    else
                    {
                        _logger.LogTrace($"Ignoring IFile.Move({file.OriginalFilename}, {targetName});");
                    }
                }
            }
        }

        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " move";
            return message;
        }
    }
}