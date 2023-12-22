using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.ErrorHandlers;
using SortPhotosWithXmpByExifDate.Operation;
using SortPhotosWithXmpByExifDate.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Performer;

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
        // when we have an error, we want to copy
        var isCopyingEnforced = true;
        var operations = new Operations(logger, _file, _directory, _isForce, isCopyingEnforced);

        if (_errorCollection.Errors.Any())
        {
            logger.LogInformation("Performing NoTimeFoundErrors");
            CollectCollisions(logger, _errorCollection.Errors,
                (FileDecomposition targetFile, NoTimeFoundError error)
                => CreateDirectoryAndCopyFile(logger, error, targetFile, operations.MoveFileOperation, operations.CopyFileOperation), operations);
        }
    }
}
