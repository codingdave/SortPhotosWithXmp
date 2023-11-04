using Microsoft.Extensions.Logging;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDateCli.Operations
{
    internal class OperationPerformerFactory
    {
        internal static IFileOperation GetCopyOrMovePerformer(ILogger logger, IFile fileWrapper, bool move, bool force)
        {
            return move 
                ? new MoveFileOperation(logger, fileWrapper, force) 
                : new CopyFileOperation(logger, fileWrapper, force);
        }
    }
}