using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Extensions;

public static class LoggerExtensions
{
    public static void LogExceptionError(this ILogger logger, Exception e)
    {
        logger.LogError(e.Message);
        logger.LogDebug(e.StackTrace);
    }

    public static void LogExceptionError(this ILogger logger, string message, Exception e)
    {
        logger.LogError(message + ": " + e.Message);
        logger.LogDebug(e.StackTrace);
    }

    public static void LogExceptionWarning(this ILogger logger, Exception e)
    {
        logger.LogWarning(e.Message);
        logger.LogDebug(e.StackTrace);
    }

    public static void LogExceptionWarning(this ILogger logger, string message, Exception e)
    {
        logger.LogWarning(message + ": " + e.Message);
        logger.LogDebug(e.StackTrace);
    }

    public static void TestInformationLevels(this ILogger logger)
    {
        logger.LogInformation($"BasePath: {Configuration.GetBasePath()}");
        logger.Log(LogLevel.Trace, "Trace messages will show up");
        logger.Log(LogLevel.Debug, "Debug messages will show up");
        logger.Log(LogLevel.Information, "Information messages will show up");
        logger.Log(LogLevel.Warning, "Warning messages will show up");
        logger.Log(LogLevel.Error, "Error messages will show up");
        logger.Log(LogLevel.Critical, "Critical messages will show up");
        logger.Log(LogLevel.None, "None messages will show up");
    }
}