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
            FileAlreadyExistsErrorCollection,
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);

        NoTimeFoundErrorPerformer = new NoTimeFoundErrorPerformer(
            NoTimeFoundErrorCollection,
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);

        MetaDataErrorPerformer = new MetaDataErrorPerformer(
            MetaDataErrorCollection,
            FilesStatistics,
            _file,
            _directory,
            destinationPath,
            isForce);
    }

    public IFilesStatistics FilesStatistics { get; } = new FilesStatistics();

    public CleanupPerformer CleanupPerformer { get; internal set; } = new CleanupPerformer();

    public IErrorCollection<NoTimeFoundError> NoTimeFoundErrorCollection { get; } = new ErrorCollection<NoTimeFoundError>();

    public IErrorCollection<MetaDataError> MetaDataErrorCollection { get; } = new ErrorCollection<MetaDataError>();

    public IErrorCollection<FileAlreadyExistsError> FileAlreadyExistsErrorCollection { get; } = new ErrorCollection<FileAlreadyExistsError>();
    public IErrorCollection<ImageProcessingExceptionError> ImageProcessingExceptionErrorCollection { get; }
    public IErrorCollection<ImageProcessingExceptionError> GeneralExceptionErrorCollection { get; }
    public IErrorCollection<ExceptionErrorBase> ExceptionCollection { get; } = new ErrorCollection<ExceptionErrorBase>();

    public NoTimeFoundErrorPerformer NoTimeFoundErrorPerformer { get; }

    public MetaDataErrorPerformer MetaDataErrorPerformer { get; }

    public FileAlreadyExistsErrorPerformer FileAlreadyExistsErrorPerformer { get; }

    public IPerformerCollection PerformerCollection { get; } = new PerformerCollection();

    public void Log(ILogger logger)
    {
        foreach (var error in ExceptionCollection.Errors)
        {
            switch (error)
            {
                case GeneralExceptionError:
                case ImageProcessingExceptionError:
                    logger.LogExceptionError(error.File, error.Exception);
                    break;
                default:
                    throw new NotImplementedException($"{nameof(error)}: {error}");
            }

        }
    }
}
