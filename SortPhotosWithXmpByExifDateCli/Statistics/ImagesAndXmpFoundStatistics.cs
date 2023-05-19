namespace SortPhotosWithXmpByExifDateCli.Statistics;


public class ImagesAndXmpFoundStatistics : IStatistics, IModifiableErrorCollection
{
    private readonly bool _force;
    public ImagesAndXmpFoundStatistics(bool force) => _force = force;
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public IReadOnlyFileError ReadOnlyFileError => FileError;
    public FileError FileError { get; } = new FileError();

    public string PrintStatistics()
    {
        var ret = "-> Found ";
        if (_force)
        {
            ret += $"and moved {FoundImages} images and {FoundXmps} xmps";
        }
        else
        {
            ret += $"{FoundImages} images and {FoundXmps} xmps. Since we are running in dry mode no movement has been performed";
        }

        foreach (var error in ReadOnlyFileError.Errors)
        {
            ret += Environment.NewLine + "*** Error: " + error.ErrorMessage;
        }
        
        return ret;
    }
}
