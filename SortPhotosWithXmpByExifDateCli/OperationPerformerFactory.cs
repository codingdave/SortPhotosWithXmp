using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli
{
    internal class OperationPerformerFactory
    {
        internal static IFileOperation GetOperationPerformer(ILogger logger, bool force, bool move)
        {
            if (force)
            {
                return new MoveOperationPerformer(logger, force);
            }
            else
            {
                return new CopyOperationPerformer(logger, force);
            }
        }
    }
}