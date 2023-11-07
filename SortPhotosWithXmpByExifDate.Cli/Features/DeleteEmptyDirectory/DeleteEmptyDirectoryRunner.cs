using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Statistics;

using SystemInterface.IO;
namespace SortPhotosWithXmpByExifDate.Cli.Features.DeleteEmptyDirectory;

public class DeleteEmptyDirectoryRunner : IRun
{
    private readonly IDirectory _directory;

    private readonly string _path;
    private readonly bool _force;
    public DeleteEmptyDirectoryRunner(IDirectory directory, string path, bool force) =>
        (_directory, _path, _force) = (directory, path, force);
        
    public IStatistics Run(ILogger logger)
    {
        var deleteDirectoryPerformer = new DeleteDirectoryOperation(logger, _directory, _force);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _directory, _path, deleteDirectoryPerformer);
        return deleteDirectoryPerformer.Statistics;
    }
}