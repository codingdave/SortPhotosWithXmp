using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class FilesFoundResult : IResult
{
    private readonly IFile _file;
    private readonly IDirectory _directory;

    public FilesFoundResult(IFile file, IDirectory directory, string destinationPath, bool isForce)
    {
        _file = file;
        _directory = directory;

        FileAlreadyExistsErrorPerformer = new FileAlreadyExistsErrorPerformer(
            FileAlreadyExistsErrors,
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);

        NoTimeFoundErrorPerformer = new NoTimeFoundErrorPerformer(
            NoTimeFoundErrors,
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);

        MetaDataErrorPerformer = new MetaDataErrorPerformer(
            MetaDataErrors,
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);
    }

    public IFilesStatistics FilesStatistics { get; } = new FilesStatistics();

    public CleanupPerformer CleanupPerformer { get; internal set; } = new CleanupPerformer();

    public IErrorCollection<FileAlreadyExistsError> FileAlreadyExistsErrors { get; } = new ErrorCollection<FileAlreadyExistsError>();
    public IErrorCollection<GeneralExceptionError> GeneralExceptionErrors { get; } = new ErrorCollection<GeneralExceptionError>();
    public IErrorCollection<ImageProcessingExceptionError> ImageProcessingExceptionErrors { get; } = new ErrorCollection<ImageProcessingExceptionError>();
    public IErrorCollection<MetaDataError> MetaDataErrors { get; } = new ErrorCollection<MetaDataError>();
    public IErrorCollection<NoTimeFoundError> NoTimeFoundErrors { get; } = new ErrorCollection<NoTimeFoundError>();

    public NoTimeFoundErrorPerformer NoTimeFoundErrorPerformer { get; }

    public MetaDataErrorPerformer MetaDataErrorPerformer { get; }

    public FileAlreadyExistsErrorPerformer FileAlreadyExistsErrorPerformer { get; }

    public IPerformerCollection Performers { get; } = new PerformerCollection();

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
