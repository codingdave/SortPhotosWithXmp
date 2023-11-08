using Microsoft.Extensions.Logging;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    internal class OperationFactory
    {
        internal static FileOperationBase GetCopyOrMoveOperation(ILogger logger, IFile file, IDirectory directory, bool isMove, bool force)
        {
            return isMove 
                ? new MoveFileOperation(logger, file, directory, force) 
                : new CopyFileOperation(logger, file, directory, force);
        }
    }
}