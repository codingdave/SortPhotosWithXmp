using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;


namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class FileAlreadyExistsErrorPerformer : ErrorPerformerBase<FileAlreadyExistsError>
{
    public FileAlreadyExistsErrorPerformer(
        IErrorCollection<FileAlreadyExistsError> errorCollection,
        IFilesStatistics foundStatistics,
        IFile file,
        IDirectory directory,
        string baseDir,
        bool isForce) : base(errorCollection, foundStatistics, file, directory, baseDir, isForce)
    {
    }

    public override void Perform(ILogger logger)
    {
        if (_errorCollection.Errors.Any())
        {
            logger.LogInformation("Performing FileAlreadyExistsErrors");
            CollectCollisions(logger, _errorCollection.Errors,
                (FileDecomposition targetFile, FileAlreadyExistsError error)
                => HandleCollisionOrDuplicate(logger, error, targetFile));
        }
    }
}