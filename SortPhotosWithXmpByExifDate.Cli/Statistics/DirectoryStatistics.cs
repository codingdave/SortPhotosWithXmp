namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class DirectoryStatistics
{
    public List<string> DirectoriesFound { get; } = new List<string>();
    public List<string> DirectoriesDeleted { get; } = new List<string>();

    public override string ToString()
    {
        var prefix = Environment.NewLine + " * ";

        var msg = $"Directories found {DirectoriesFound.Count}, deleted {DirectoriesDeleted.Count}";
        msg += Environment.NewLine;
        msg += $"Found: {prefix}{string.Join(prefix, DirectoriesFound)}";
        msg += Environment.NewLine;
        msg += $"Deleted: {prefix}{string.Join(prefix, DirectoriesDeleted)}";
        return msg;
    }
}
