using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;
namespace SortPhotosWithXmpByExifDateCli;

public class DeleteEmptyDirectory : IRun 
{
    private readonly DirectoryInfo _directory;
    private readonly bool _force;
    public DeleteEmptyDirectory(DirectoryInfo directory, bool force) => 
        (_directory, _force) = (directory, force);
    public IStatistics Run(ILogger logger)
    {
        var statistics = new DirectoriesDeletedStatistics(logger, _force);
        Helpers.RecursivelyDeleteEmptyDirectories(_directory, statistics, _force);
        return statistics;
    }
}