namespace SortPhotosWithXmpByExifDateCli.Statistics;


public class ImagesAndXmpFoundStatistics : IStatistics, IModifiableErrorCollection
{
    private readonly bool _force;
    public ImagesAndXmpFoundStatistics(bool force) => _force = force;
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public IReadOnlyFileError ReadOnlyFileError => FileError;
    public FileError FileError { get; } = new FileError();

    public string PrintStatistics(bool move)
    {
        var ret = "-> Found ";
        if (_force)
        {
            var operation = move ? "moved" : "copied";
            ret += $"and {operation} {FoundImages} images and {FoundXmps} xmps";
        }
        else
        {
            ret += $"{FoundImages} images and {FoundXmps} xmps. Since we are running in dry mode no action has been performed";
        }

        foreach (var error in ReadOnlyFileError.Errors)
        {
            ret += Environment.NewLine + "*** Error for " + error.FileInfo + ". " + error.ErrorMessage;
        }

        return ret;
    }
}
