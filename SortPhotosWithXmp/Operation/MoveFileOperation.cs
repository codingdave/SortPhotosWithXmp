using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;

using SortPhotosWithXmp.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Operation;

public class MoveFileOperation : FileOperationBase
{
    private readonly IFile _file;

    internal MoveFileOperation(ILogger logger, IFile file, IDirectory directory, Action<FileAlreadyExistsError> handleError, bool isForce)
    : base(logger, directory, handleError, isForce)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }

    public override void ChangeFiles(IEnumerable<IImageFile> files, string targetPath)
    {
        void Errorhandler(string sourceFileName, string destFileName)
        {
            _logger.LogTrace($"MoveFileOperation({sourceFileName}, {destFileName});");
            _file.Move(sourceFileName, destFileName);
        };

        ChangeFiles(files, targetPath, Errorhandler);
    }

    public void RenameDirectory(string sourceDirName, string destDirName)
    {
        _directory.Move(sourceDirName, destDirName);
    }
}