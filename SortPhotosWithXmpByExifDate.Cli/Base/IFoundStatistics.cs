namespace SortPhotosWithXmpByExifDate.Cli.Result;

public interface IFoundStatistics
{
    int FoundXmps { get; set; }
    int FoundImages { get; set; }
    int SkippedXmps { get; set; }
    int SkippedImages { get; set; }
}
