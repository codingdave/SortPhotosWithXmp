using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;

using SortPhotosWithXmp.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Operation;

public class MoveFileOperation : FileOperationBase
{
    internal MoveFileOperation(ILogger logger, IFile fileWrapper, IDirectory directoryWrapper, Action<FileAlreadyExistsError> handleError, bool isForce)
    : base(logger, fileWrapper, directoryWrapper, handleError, isForce)
    {
    }

    public override void ChangeFiles(IEnumerable<IImageFile> files, string targetPath)
    {
        void Errorhandler(string sourceFileName, string destFileName)
        {
            _logger.LogTrace($"MoveFileOperation({sourceFileName}, {destFileName});");
            _fileWrapper.Move(sourceFileName, destFileName);
        };

        ChangeFiles(files, targetPath, Errorhandler);
    }

    public void RenameDirectory(string sourceDirName, string destDirName)
    {
        _directoryWrapper.Move(sourceDirName, destDirName);
    }
}