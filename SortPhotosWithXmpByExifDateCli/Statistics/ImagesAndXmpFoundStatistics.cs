using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ImagesAndXmpFoundStatistics : IStatistics, IModifiableErrorCollection
{
    private readonly ILogger _logger;
    private readonly bool _force;
    private readonly bool _move;
    public ImagesAndXmpFoundStatistics(ILogger logger, bool force, bool move) =>
    (_logger, _force, _move, _errors) = (logger, force, move, new ErrorCollection(logger));

    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public IReadOnlyErrorCollection FileErrors => _errors;
    private readonly IErrorCollection _errors;

    public void AddError(IError error)
    {
        _errors.Add(error);
    }

    public void Log()
    {
        if (_force)
        {
            var operation = _move ? "moved" : "copied";
            _logger.LogInformation("-> Found and {operation} {FoundImages} images and {FoundXmps} xmps", operation, FoundImages, FoundXmps);
        }
        else
        {
            _logger.LogInformation("-> Found {FoundImages} images and {FoundXmps} xmps. Since we are running in dry mode no action has been performed", FoundImages, FoundXmps);
        }

        foreach (var error in FileErrors.Errors)
        {
            switch (error)
            {
                case MetaDataError me:
                    _logger.LogTrace("{FileInfo}. {ErrorMessage}", me.FileInfo, me.ErrorMessage);
                    break;
                case NoTimeFoundError:
                case ImageProcessingExceptionError:
                    _logger.LogError("{FileInfo}. {ErrorMessage}", error.FileInfo, error.ErrorMessage);
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
