using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ImagesAndXmpFoundStatistics : IStatistics, IModifiableErrorCollection
{
    private readonly bool _force;
    private readonly bool _move;
    public ImagesAndXmpFoundStatistics(bool force, bool move) => (_force, _move) = (force, move);
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public IReadOnlyFileError FileError => _fileError;
    private IFileError _fileError = new FileError();

    public void AddError(IError error)
    {
        _fileError.Add(error);
    }

    public void Log(ILogger logger)
    {
        if (_force)
        {
            var operation = _move ? "moved" : "copied";
            logger.LogInformation("-> Found and {operation} {FoundImages} images and {FoundXmps} xmps", operation, FoundImages, FoundXmps);
        }
        else
        {
            logger.LogInformation("-> Found {FoundImages} images and {FoundXmps} xmps. Since we are running in dry mode no action has been performed", FoundImages, FoundXmps);
        }

        foreach (var error in ReadOnlyFileError.Errors)
        {
            if (
                error.ErrorMessage.StartsWith("Unsupported ilist key") ||
                error.ErrorMessage.StartsWith("ICC data describes an invalid date/time") ||
                error.ErrorMessage.StartsWith("Invalid TIFF tag format code 13 for tag 0x0011") ||
                error.ErrorMessage.StartsWith("Exception processing TIFF data: Unclear distinction between Motorola/Intel byte ordering: 17784")
            )
            {
                logger.LogTrace("{FileInfo}. {ErrorMessage}", error.FileInfo, error.ErrorMessage);
            }
            else
            {
                logger.LogError("{FileInfo}. {ErrorMessage}", error.FileInfo, error.ErrorMessage);
            }
        }
    }
}
