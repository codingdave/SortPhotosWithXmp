using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ImagesAndXmpFoundStatistics : IStatistics, IModifiableErrorCollection
{
    private readonly bool _force;
    private readonly bool _move;
    public ImagesAndXmpFoundStatistics(bool force, bool move) => (_force, _move) = (force, move);
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public IReadOnlyFileError ReadOnlyFileError => FileError;
    public FileError FileError { get; } = new FileError();

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
            logger.LogError("{FileInfo}. {ErrorMessage}", error.FileInfo, error.ErrorMessage);
        }
    }
}
