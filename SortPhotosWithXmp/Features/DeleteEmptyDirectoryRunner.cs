using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Result;

using SystemInterface.IO;
namespace SortPhotosWithXmp.Features;

public class DeleteEmptyDirectoryRunner : IRun
{
    private readonly IDirectory _directoryWrapper;

    private readonly string _path;
    public DeleteEmptyDirectoryRunner(IDirectory directoryWrapper, string path, bool isForce) =>
        (_directoryWrapper, _path, IsForce) = (directoryWrapper, path, isForce);

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
        // Delete or collect empty directories?

        // var deleteDirectoryPerformer = new DeleteDirectoryOperation(logger, _directory, _force);
        // Helpers.RecursivelyDeleteEmptyDirectories(logger, _directory, _path, deleteDirectoryPerformer);
        // return deleteDirectoryPerformer.Result;
        throw new NotImplementedException();
    }
}