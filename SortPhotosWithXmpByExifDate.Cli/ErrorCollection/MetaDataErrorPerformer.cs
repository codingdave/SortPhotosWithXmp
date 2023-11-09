using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;


namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class MetaDataErrorPerformer : ErrorPerformerBase<MetaDataError>
{
    public MetaDataErrorPerformer(
        IErrorCollection<MetaDataError> errorCollection,
        IFilesStatistics foundStatistics,
        IFile file,
        IDirectory directory,
        string baseDir,
        bool isForce) : base(errorCollection, foundStatistics, file, directory, baseDir, isForce)
    {
    }

    public override void Perform(ILogger logger)
    {
        // when we have an error, we want to copy
        var isCopyingEnforced = true;
#warning copy and move should HAVE not are a directory operator
        _copyFileOperation = new CopyFileOperation(logger, _file, _directory, isCopyingEnforced);
        _moveFileOperation = new MoveFileOperation(logger, _file, _directory, isCopyingEnforced);
        _deleteFileOperation = new DeleteFileOperation(logger, _file, _directory, _isForce);

        CollectCollisions(logger, _errorCollection.Errors,
            (FileDecomposition targetFile, MetaDataError error)
            => CreateDirectoryAndCopyFile(logger, error, targetFile));
    }
}
