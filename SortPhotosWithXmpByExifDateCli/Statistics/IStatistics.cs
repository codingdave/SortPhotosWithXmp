namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IStatistics
{
    string PrintStatistics();
    IReadOnlyFileError ReadOnlyFileError { get; }
}