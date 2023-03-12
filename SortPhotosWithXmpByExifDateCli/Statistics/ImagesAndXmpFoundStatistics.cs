namespace SortPhotosWithXmpByExifDateCli;

public class ImagesAndXmpFoundStatistics : IStatistics
{
    private bool _force;
    public ImagesAndXmpFoundStatistics(bool force) => _force = force;
    public List<string> Errors { get; } = new List<string>();
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public string PrintStatistics() 
    {
        var ret = string.Empty;
        if(_force)
        {
            ret = $"Found and moved {FoundImages} images and {FoundXmps} xmps";
        }
        else
        {
            ret = $"Found {FoundImages} images and {FoundXmps} xmps. Since we are running in dry mode no movement has been performed";
        }

        foreach(var error in Errors)
        {
            ret += error;
        }
        
        return ret;
    }
}
