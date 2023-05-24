using MetadataExtractor;
using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class DirectoriesDeletedStatistics : IStatistics
{
    private readonly bool _force;
    private readonly ILogger _logger;

    public DirectoriesDeletedStatistics(ILogger logger, bool force)
    {
        _logger = logger;
        _force = force;
        FileErrors = new ErrorCollection(logger);
    }

    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }

    public IReadOnlyErrorCollection FileErrors { get; } 

    public void Log()
    {
        var info = "-> Found {DirectoriesFound} directories";

        info += DirectoriesDeleted switch
        {
            > 0 when _force => ", deleted {DirectoriesDeleted} directories",
            _ => ", skipped deleting {DirectoriesDeleted} directories due to dry run",
        };
        _logger.LogInformation(info, DirectoriesFound, DirectoriesDeleted);

        foreach (var error in FileErrors.Errors)
        {
            _logger.LogError(error.ErrorMessage);
        }
    }
}