using Microsoft.Extensions.Logging;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    internal class OperationFactory
    {
        internal static FileOperationBase GetCopyOrMoveOperation(ILogger logger, IFile file, IDirectory directory, bool isMove, bool isForce)
        {
            return isMove 
                ? new MoveFileOperation(logger, file, directory, isForce) 
                : new CopyFileOperation(logger, file, directory, isForce);
        }
    }
}   