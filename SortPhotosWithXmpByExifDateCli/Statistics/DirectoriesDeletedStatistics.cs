using MetadataExtractor;
using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class DirectoriesDeletedStatistics : IStatistics
{
    private readonly bool _force;
    public DirectoriesDeletedStatistics(bool force)
    {
        _force = force;
    }

    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }

    public IReadOnlyFileError FileError { get; } = new FileError();

    public void Log(ILogger logger)
    {
        var info = "-> Found {DirectoriesFound} directories";

        info += DirectoriesDeleted switch
        {
            > 0 when _force => ", deleted {DirectoriesDeleted} directories",
            _ => ", skipped deleting {DirectoriesDeleted} directories due to dry run",
        };
        logger.LogInformation(info, DirectoriesFound, DirectoriesDeleted);

        foreach (var error in FileError.Errors)
        {
            logger.LogError(error.ErrorMessage);
        }
    }
}