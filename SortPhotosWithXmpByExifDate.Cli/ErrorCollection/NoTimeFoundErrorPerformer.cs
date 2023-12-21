using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;


namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class NoTimeFoundErrorPerformer : ErrorPerformerBase<NoTimeFoundError>
{
    public NoTimeFoundErrorPerformer(
        IFilesStatistics filesStatistics,
        IFile file,
        IDirectory directory,
        string baseDir,
        bool isForce)
    : base(filesStatistics, file, directory, baseDir, isForce)
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
