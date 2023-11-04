using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Operations
{
    internal class OperationPerformerFactory
    {
        internal static IFileOperation GetCopyOrMovePerformer(ILogger logger, bool move, bool force)
        {
            return move 
                ? new MoveFileOperation(logger, force) 
                : new CopyFileOperation(logger, force);
        }
    }
}