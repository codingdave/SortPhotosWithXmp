using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class FilesFoundStatistics : IStatistics, IModifiableErrorCollection, IFoundStatistics, IFileOperationStatistics
{
    private readonly ILogger _logger;
    public IFileOperation FileOperation { get; }
    public FilesFoundStatistics(ILogger logger, IFileOperation fileOperation) =>
    (_logger, FileOperation, _errors) = (logger, fileOperation, new ErrorCollection(logger));

    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }
    public int SkippedXmps { get; set; }
    public int SkippedImages { get; set; }

    public IReadOnlyErrorCollection FileErrors => _errors;
    private readonly IErrorCollection _errors;

    public void AddError(IError error)
    {
        _errors.Add(error);
    }

    public void Log()
    {
        // if (_operationPerformer)
        var operation = FileOperation.ToString(); // performing/simulating move/copy
        _logger.LogInformation("-> {operation}. Found {FoundImages} individual images and {FoundXmps} xmps ({SkippedImages}/{SkippedXmps} duplicates).", operation, FoundImages, FoundXmps, SkippedImages, SkippedXmps);

        foreach (var error in FileErrors.Errors)
        {
            switch (error)
            {
                case MetaDataError me:
                    _logger.LogTrace("{FileInfo}. {ErrorMessage}", me.FileInfo, me.ErrorMessage);
                    break;
                case NoTimeFoundError:
                    _logger.LogError("{FileInfo}. {ErrorMessage}", error.FileInfo, error.ErrorMessage + Environment.NewLine + error);
                    break;
                case ImageProcessingExceptionError ipe:
                    _logger.LogError(error.FileInfo.FullName, ipe);
                    break;
                case FileAlreadyExistsError:
                    // nothing to do over here
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
