using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Operations;
using SortPhotosWithXmpByExifDateCli.Statistics;

using SystemInterface.IO;
namespace SortPhotosWithXmpByExifDateCli.Features.DeleteEmptyDirectory;

public class DeleteEmptyDirectoryRunner : IRun
{
    private readonly IDirectory _directoryWrapper;

    private readonly string _directory;
    private readonly bool _force;
    public DeleteEmptyDirectoryRunner(IDirectory directoryWrapper, string directory, bool force) =>
        (_directoryWrapper, _directory, _force) = (directoryWrapper, directory, force);
    public IStatistics Run(ILogger logger)
    {
        var deleteDirectoryPerformer = new DeleteDirectoryOperation(logger, _directoryWrapper, _force);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _directory, deleteDirectoryPerformer);
        return deleteDirectoryPerformer.Statistics;
    }
}