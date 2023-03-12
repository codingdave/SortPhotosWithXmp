namespace SortPhotosWithXmpByExifDateCli.Statistics;


public class ImagesAndXmpFoundStatistics : IStatistics, IErrors
{
    private bool _force;
    public ImagesAndXmpFoundStatistics(bool force) => _force = force;
    public IErrorCollection ErrorCollection { get; } = new ErrorCollection();
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public string PrintStatistics() 
    {
        var ret = string.Empty;
        if (_force)
        {
            ret += $"Found and moved {FoundImages} images and {FoundXmps} xmps";
        }
        else
        {
            ret += $"Found {FoundImages} images and {FoundXmps} xmps. Since we are running in dry mode no movement has been performed";
        }

        ret += string.Join(System.Environment.NewLine, ErrorCollection.Errors);
        
        return ret;
    }
}
