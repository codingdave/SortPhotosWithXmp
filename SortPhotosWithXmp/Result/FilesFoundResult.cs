using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Performer;
using SortPhotosWithXmp.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Result;

public class FilesFoundResult : IResult
{
    private readonly IFile _fileWrapper;
    private readonly IDirectory _directoryWrapper;

    public FilesFoundResult(ILogger logger, IFile fileWrapper, IDirectory directoryWrapper, string destinationPath, bool isForce)
    {
        _fileWrapper = fileWrapper;
        _directoryWrapper = directoryWrapper;

        FileAlreadyExistsErrorPerformer = new FileAlreadyExistsErrorPerformer(
            logger,
            FilesStatistics,
            _fileWrapper,
            _directoryWrapper,
            destinationPath,
            isForce);

        NoTimeFoundErrorPerformer = new NoTimeFoundErrorPerformer(
            FilesStatistics,
            _fileWrapper,
            _directoryWrapper,
            destinationPath,
            isForce);

        MetaDataErrorPerformer = new MetaDataErrorPerformer(
            FilesStatistics,
            _fileWrapper,
            _directoryWrapper,
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
            logger.LogExceptionError(error.FileName, error.Exception);
        }

        foreach (var error in ImageProcessingExceptionErrors.Errors)
        {
            logger.LogExceptionError(error.FileName, error.Exception);
        }
    }
}
