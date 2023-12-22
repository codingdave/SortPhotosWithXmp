using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;
using SortPhotosWithXmp.Operation;
using SortPhotosWithXmp.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Performer;

public class MetaDataErrorPerformer : ErrorPerformerBase<MetaDataError>
{
    public MetaDataErrorPerformer(
        IFilesStatistics filesStatistics,
        IFile file,
        IDirectory directory,
        string baseDir,
        bool isForce) : base(filesStatistics, file, directory, baseDir, isForce)
    {
    }

    public override void Perform(ILogger logger)
    {
        if (_errorCollection.Errors.Any())
        {
            logger.LogInformation("Performing MetaDataErrors");
            // when we have an error, we want to copy
            var isCopyingEnforced = true;
            var operations = new Operations(logger, _file, _directory, _isForce, isCopyingEnforced);

            CollectCollisions(logger, _errorCollection.Errors,
                (FileDecomposition targetFile, MetaDataError error)
                => CreateDirectoryAndCopyFile(logger, error, targetFile, operations.MoveFileOperation, operations.CopyFileOperation), operations);
        }
    }
}
