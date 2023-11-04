using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Operations;
using SortPhotosWithXmpByExifDateCli.Statistics;
namespace SortPhotosWithXmpByExifDateCli.Features.DeleteEmptyDirectory;

public class DeleteEmptyDirectoryRunner : IRun
{
    private readonly string _directory;
    private readonly bool _force;
    public DeleteEmptyDirectoryRunner(string directory, bool force) =>
        (_directory, _force) = (directory, force);
    public IStatistics Run(ILogger logger)
    {
        var deleteDirectoryPerformer = new DeleteDirectoryOperation(logger, _force);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _directory, deleteDirectoryPerformer);
        return deleteDirectoryPerformer.Statistics;
    }
}