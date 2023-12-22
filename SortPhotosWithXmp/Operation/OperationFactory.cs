using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Operation;

internal class OperationFactory
{
    internal static FileOperationBase GetCopyOrMoveOperation(ILogger logger, IFile file, IDirectory directory, Action<FileAlreadyExistsError> handleError, bool isMove, bool isForce)
    {
        return isMove
            ? new MoveFileOperation(logger, file, directory, handleError, isForce)
            : new CopyFileOperation(logger, file, directory, handleError, isForce);
    }
}