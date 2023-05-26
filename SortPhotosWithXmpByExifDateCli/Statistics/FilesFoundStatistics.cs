using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class FilesFoundStatistics : IStatistics, IModifiableErrorCollection, IFoundStatistics
{
    private readonly ILogger _logger;
    private readonly IFileOperation _operationPerformer;
    public FilesFoundStatistics(ILogger logger, IFileOperation operationPerformer) =>
    (_logger, _operationPerformer, _errors) = (logger, operationPerformer, new ErrorCollection(logger));

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
        var operation = _operationPerformer.ToString(); // performing/simulating move/copy
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
                    _logger.LogError("{FileInfo}. {ErrorMessage}", error.FileInfo, error.ErrorMessage);
                    _logger.LogTrace("{Stacktrace}", ipe.Exception.StackTrace);
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
