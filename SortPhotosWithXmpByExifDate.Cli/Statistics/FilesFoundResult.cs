using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.ErrorHandlers;
using SortPhotosWithXmpByExifDate.Extensions;
using SortPhotosWithXmpByExifDate.Performer;
using SortPhotosWithXmpByExifDate.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Result;

public class FilesFoundResult : IResult
{
    private readonly IFile _file;
    private readonly IDirectory _directory;

    public FilesFoundResult(ILogger logger, IFile file, IDirectory directory, string destinationPath, bool isForce)
    {
        _file = file;
        _directory = directory;

        FileAlreadyExistsErrorPerformer = new FileAlreadyExistsErrorPerformer(
            logger,
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);

        NoTimeFoundErrorPerformer = new NoTimeFoundErrorPerformer(
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);

        MetaDataErrorPerformer = new MetaDataErrorPerformer(
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);
    }

    public CleanupPerformer CleanupPerformer { get; internal set; } = new CleanupPerformer();
    public FileAlreadyExistsErrorPerformer FileAlreadyExistsErrorPerformer { get; }
    public IErrorCollection<GeneralExceptionError> GeneralExceptionErrors { get; } = new ErrorCollection<GeneralExceptionError>();
    public IErrorCollection<ImageProcessingExceptionError> ImageProcessingExceptionErrors { get; } = new ErrorCollection<ImageProcessingExceptionError>();
    public IFilesStatistics FilesStatistics { get; } = new FilesStatistics();
    public IPerformerCollection Performers { get; } = new PerformerCollection();
    public MetaDataErrorPerformer MetaDataErrorPerformer { get; }
    public NoTimeFoundErrorPerformer NoTimeFoundErrorPerformer { get; }
    public void Log(ILogger logger)
    {
        foreach (var error in GeneralExceptionErrors.Errors)
        {
            logger.LogExceptionError(error.File, error.Exception);
        }

        foreach (var error in ImageProcessingExceptionErrors.Errors)
        {
            logger.LogExceptionError(error.File, error.Exception);
        }
    }
}
