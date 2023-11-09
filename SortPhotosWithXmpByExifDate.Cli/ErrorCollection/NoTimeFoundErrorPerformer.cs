using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;


namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class NoTimeFoundErrorPerformer : ErrorPerformerBase<NoTimeFoundError>
{
    public NoTimeFoundErrorPerformer(
        IErrorCollection<NoTimeFoundError> errorCollection,
        IFilesStatistics foundStatistics,
        IFile file,
        IDirectory directory,
        string baseDir,
        bool isForce)
    : base(errorCollection, foundStatistics, file, directory, baseDir, isForce)
    {
    }

    public override void Perform(ILogger logger)
    {
        if (_errorCollection.Errors.Any())
        {
            logger.LogInformation("Performing NoTimeFoundErrors");
            CollectCollisions(logger, _errorCollection.Errors,
                (FileDecomposition targetFile, NoTimeFoundError error)
                => CreateDirectoryAndCopyFile(logger, error, targetFile));
        }
    }
}
