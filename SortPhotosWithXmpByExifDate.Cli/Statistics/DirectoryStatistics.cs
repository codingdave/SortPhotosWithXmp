namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class DirectoryStatistics
{
    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }
    
    public override string ToString()
    {
        return $"Found {DirectoriesFound}, deleted {DirectoriesDeleted} directories";
    }
}
