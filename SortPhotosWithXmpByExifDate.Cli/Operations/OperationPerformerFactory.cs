using Microsoft.Extensions.Logging;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    internal class OperationPerformerFactory
    {
        internal static IFileOperation GetCopyOrMovePerformer(ILogger logger, IFile file, bool move, bool force)
        {
            return move 
                ? new MoveFileOperation(logger, file, force) 
                : new CopyFileOperation(logger, file, force);
        }
    }
}