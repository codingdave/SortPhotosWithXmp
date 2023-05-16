namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IStatistics
{
    string PrintStatistics();
    IReadOnlyErrorCollection ErrorCollection { get; }
}