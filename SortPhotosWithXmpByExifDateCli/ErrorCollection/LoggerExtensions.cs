using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class LoggerExtensions
    {
        public static void LogExceptionError(this ILogger logger, Exception e)
        {
            logger.LogError(e.Message);
            logger.LogTrace(e.StackTrace);
        }

        public static void LogExceptionError(this ILogger logger, string message, Exception e)
        {
            logger.LogError(message + ": " + e.Message);
            logger.LogTrace(e.StackTrace);
        }

        public static void LogExceptionWarning(this ILogger logger, Exception e)
        {
            logger.LogWarning(e.Message);
            logger.LogTrace(e.StackTrace);
        }

        public static void LogExceptionWarning(this ILogger logger, string message, Exception e)
        {
            logger.LogWarning(message + ": " + e.Message);
            logger.LogTrace(e.StackTrace);
        }
    }
}