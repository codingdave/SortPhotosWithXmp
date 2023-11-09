using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class MoveFileOperation : FileOperationBase
    {
        private readonly ILogger _logger;
        private readonly IFile _file;

        internal MoveFileOperation(ILogger logger, IFile file, IDirectory directory, bool isForce)
        : base(directory, isForce)
        {
            _logger = logger;
            _file = file;
        }

        public override void ChangeFiles(IEnumerable<IImageFile> files, string targetPath)
        {
            CreateDirectory(targetPath);

            foreach (var file in files)
            {
                var targetName = JoinFile(targetPath, Path.GetFileName(file.CurrentFilename));

                if (File.Exists(targetName))
                {
                    var error = new FileAlreadyExistsError(targetName, file.CurrentFilename, $"File {file.CurrentFilename} already exists at {targetName}");
                    _logger.LogError(error.ToString());
                }
                else
                {
                    if (IsForce)
                    {
                        try
                        {
                            _logger.LogTrace($"IFile.Move({file.CurrentFilename}, {targetName});");
                            _file.Move(file.CurrentFilename, targetName);
                        }
                        catch (Exception e)
                        {
                            _logger.LogExceptionError(e);
                        }
                    }
                    else
                    {
                        _logger.LogTrace($"Ignoring IFile.Move({file.CurrentFilename}, {targetName});");
                    }
                    file.NewFilename = targetName;
                }
            }
        }


        public void RenameDirectory(string sourceDirName, string destDirName)
        {
            _directory.Move(sourceDirName, destDirName);
        }
    }
}