using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;


namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

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
            CollectCollisions(logger, _errorCollection.Errors,
                (FileDecomposition targetFile, MetaDataError error)
                => CreateDirectoryAndCopyFile(logger, error, targetFile));
        }
    }
}
