namespace SortPhotosWithXmpByExifDateCli;

public interface IStatistics {
    string PrintStatistics();
    public List<string> Errors { get; }
 }
