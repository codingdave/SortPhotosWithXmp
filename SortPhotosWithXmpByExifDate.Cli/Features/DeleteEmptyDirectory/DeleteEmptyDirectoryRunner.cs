using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;
namespace SortPhotosWithXmpByExifDate.Cli.Features.DeleteEmptyDirectory;

public class DeleteEmptyDirectoryRunner : IRun
{
    private readonly IDirectory _directory;

    private readonly string _path;
    public DeleteEmptyDirectoryRunner(IDirectory directory, string path, bool isForce) =>
        (_directory, _path, IsForce) = (directory, path, isForce);

    public bool IsForce { get; }

    public IResult Run(ILogger logger)
    {
#warning Collect Empty Directories?
        // var deleteDirectoryPerformer = new DeleteDirectoryOperation(logger, _directory, _force);
        // Helpers.RecursivelyDeleteEmptyDirectories(logger, _directory, _path, deleteDirectoryPerformer);
        // return deleteDirectoryPerformer.Result;
        throw new NotImplementedException();
    }
}