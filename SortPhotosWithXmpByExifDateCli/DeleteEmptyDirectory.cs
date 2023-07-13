using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Operation;
using SortPhotosWithXmpByExifDateCli.Statistics;
namespace SortPhotosWithXmpByExifDateCli;

public class DeleteEmptyDirectory : IRun 
{
    private readonly string _directory;
    private readonly bool _force;
    public DeleteEmptyDirectory(string directory, bool force) => 
        (_directory, _force) = (directory, force);
    public IStatistics Run(ILogger logger)
    {
        var deleteDirectoryPerformer = new DeleteDirectoryOperation(logger, _force);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _directory, deleteDirectoryPerformer);
        return deleteDirectoryPerformer.Statistics;
    }
}