using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public class FilesFoundResult : IResult, IModifiableErrorCollection, IFoundStatistics, IFileOperationStatistics
{
    private readonly ILogger _logger;
    public IFileOperation FileOperation { get; }
    public FilesFoundResult(ILogger logger, IFileOperation fileOperation)
        => (_logger, FileOperation, _errorCollection, _successfulCollection) = (logger, fileOperation, new ErrorCollection.ErrorCollection(logger), new SuccessCollection());

    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }
    public int SkippedXmps { get; set; }
    public int SkippedImages { get; set; }

    public IReadOnlyErrorCollection ErrorCollection => _errorCollection;
    private readonly IErrorCollection _errorCollection;

    public IReadOnlySuccessCollection SuccessfulCollection => _successfulCollection;
    private readonly ISuccessCollection _successfulCollection;


    public void AddError(IError error)
    {
        _errorCollection.Add(error);
    }

    public void AddSuccessful(ISuccess successful)
    {
        _successfulCollection.Add(successful);
    }

    public void Log()
    {
        var operation = FileOperation.ToString(); // performing/simulating move/copy
        _logger.LogInformation("-> {operation}. Found {FoundImages} individual images and {FoundXmps} xmps ({SkippedImages}/{SkippedXmps} duplicates).", operation, FoundImages, FoundXmps, SkippedImages, SkippedXmps);

        foreach (var error in ErrorCollection.Errors)
        {
            switch (error)
            {
                case FileAlreadyExistsError:
                    // nothing to do over here
                    break;
                case ImageProcessingExceptionError ipe:
                    _logger.LogExceptionError(error.File, ipe.Exception);
                    break;
                case GeneralExceptionError:
                case MetaDataError:
                case NoTimeFoundError:
                    _logger.LogError(error.ToString());
                    break;
                default:
                    throw new NotImplementedException($"{error}");
            }
        }
    }
}
