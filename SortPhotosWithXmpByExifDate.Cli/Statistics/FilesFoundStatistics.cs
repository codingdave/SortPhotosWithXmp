using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public class FilesFoundStatistics : IStatistics, IModifiableErrorCollection, IFoundStatistics, IFileOperationStatistics
{
    private readonly ILogger _logger;
    public IFileOperation FileOperation { get; }
    public FilesFoundStatistics(ILogger logger, IFileOperation fileOperation)
    {
        (_logger, FileOperation, _errors) = (logger, fileOperation, new ErrorCollection.ErrorCollection(logger));
    }

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
        var operation = FileOperation.ToString(); // performing/simulating move/copy
        _logger.LogInformation("-> {operation}. Found {FoundImages} individual images and {FoundXmps} xmps ({SkippedImages}/{SkippedXmps} duplicates).", operation, FoundImages, FoundXmps, SkippedImages, SkippedXmps);

        foreach (var error in FileErrors.Errors)
        {
            switch (error)
            {
                case MetaDataError me:
                    _logger.LogTrace(error.ToString());
                    break;
                case NoTimeFoundError:
                    _logger.LogError(error.ToString());
                    break;
                case ImageProcessingExceptionError ipe:
                    _logger.LogExceptionError(error.File, ipe.Exception);
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
