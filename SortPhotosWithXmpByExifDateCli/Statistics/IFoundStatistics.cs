namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IFoundStatistics: IFileOperationStatistics
{
    int FoundXmps { get; set; }
    int FoundImages { get; set; }
    int SkippedXmps { get; set; }
    int SkippedImages { get; set; }
}
