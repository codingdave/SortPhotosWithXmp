namespace SortPhotosWithXmpByExifDateCli;

public class DirectoriesDeletedStatistics : IStatistics
{
    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }
    public string PrintStatistics() => $"Found {DirectoriesFound} directories, deleted {DirectoriesDeleted} directories";
}