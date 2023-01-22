namespace SortPhotosWithXmpByExifDateCli;

public class Statistics
{
    public int FoundXmps { get; set; }
    public int FoundImages { get; set; }

    public override string ToString() => $"Found {FoundImages} images and {FoundXmps} xmps";
}