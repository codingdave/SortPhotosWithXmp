using Microsoft.Extensions.Logging;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    internal class OperationPerformerFactory
    {
        internal static ICopyOrMoveFileOperation GetCopyOrMovePerformer(ILogger logger, IFile file, IDirectory directory, bool move, bool force)
        {
            return move 
                ? new MoveFileOperation(logger, file, directory, force) 
                : new CopyFileOperation(logger, file, directory, force);
        }
    }
}