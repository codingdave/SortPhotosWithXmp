using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Operation;

internal class OperationFactory
{
    internal static FileOperationBase GetCopyOrMoveOperation(
        ILogger logger,
        IFile fileWrapper,
        IDirectory directoryWrapper,
        Action<FileAlreadyExistsError> handleError,
        bool isMove,
        bool isForce)
    {
        return isMove
            ? new MoveFileOperation(logger, fileWrapper, directoryWrapper, handleError, isForce)
            : new CopyFileOperation(logger, fileWrapper, directoryWrapper, handleError, isForce);
    }
}