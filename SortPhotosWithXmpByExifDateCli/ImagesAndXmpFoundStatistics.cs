namespace SortPhotosWithXmpByExifDateCli;

public class ImagesAndXmpFoundStatistics : IStatistics
{
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public string PrintStatistics() => $"Found {FoundImages} images and {FoundXmps} xmps";
}
