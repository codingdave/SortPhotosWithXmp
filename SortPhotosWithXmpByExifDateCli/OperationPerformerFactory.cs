using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli
{
    internal class OperationPerformerFactory
    {
        internal static IFileOperation GetOperationPerformer(ILogger logger, bool force, bool move)
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