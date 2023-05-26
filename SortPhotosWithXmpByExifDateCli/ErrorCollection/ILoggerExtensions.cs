using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class ILoggerExtensions
    {
        public static void LogError(this ILogger logger, Exception e)
        {
            logger.LogError(e.Message);
            logger.LogTrace(e.StackTrace);
        }

        public static void LogError(this ILogger logger, string message, Exception e)
        {
            logger.LogError(message + ": " + e.Message);
            logger.LogTrace(e.StackTrace);
        }

        public static void LogWarning(this ILogger logger, Exception e)
        {
            logger.LogWarning(e.Message);
            logger.LogTrace(e.StackTrace);
        }

        public static void LogWarning(this ILogger logger, string message, Exception e)
        {
            logger.LogWarning(message + ": " + e.Message);
            logger.LogTrace(e.StackTrace);
        }
    }
}