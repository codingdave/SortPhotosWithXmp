using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

namespace SortPhotosWithXmpByExifDate.Cli.Result;



public class FilesFoundResult : IResult, IModifiableErrorCollection, IFoundStatistics
{
    private readonly ILogger _logger;
    public FilesFoundResult(ILogger logger)
        => (_logger, _errorCollection, _successfulCollection)
        = (logger, new ErrorCollection.ErrorCollection(logger), new SuccessCollection());

    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }
    public int SkippedXmps { get; set; }
    public int SkippedImages { get; set; }

    public IReadOnlyErrorCollection ErrorCollection => _errorCollection;
    private readonly IErrorCollection _errorCollection;

    public IReadOnlySuccessCollection SuccessfulCollection => _successfulCollection;

    public DirectoriesDeletedResult CleanupResult { get; internal set; }

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
        _logger.LogInformation("-> Found images: {FoundImages}, xmps: {FoundXmps} and duplicates {SkippedImages}/{SkippedXmps}.", FoundImages, FoundXmps, SkippedImages, SkippedXmps);

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
