using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Operations
{
    internal class OperationPerformerFactory
    {
        internal static IFileOperation GetCopyOrMovePerformer(ILogger logger, bool move, bool force)
        {
            if (move)
            {
                return new MoveFileOperation(logger, force);
            }
            else
            {
                return new CopyFileOperation(logger, force);
            }
        }
    }
}