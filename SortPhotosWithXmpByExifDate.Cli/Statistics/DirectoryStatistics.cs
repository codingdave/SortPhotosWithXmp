namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class DirectoryStatistics
{
    public List<string> DirectoriesFound { get; } = new List<string>();
    public List<string> DirectoriesDeleted { get; } = new List<string>();

    public override string ToString()
    {
        return $"Found {DirectoriesFound.Count}, deleted {DirectoriesDeleted.Count} directories";
    }
}
