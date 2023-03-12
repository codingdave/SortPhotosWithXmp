namespace SortPhotosWithXmpByExifDateCli;

public class DirectoriesDeletedStatistics : IStatistics
{
    private bool _force;
    public DirectoriesDeletedStatistics(bool force)
    {
        _force = force;
    }

    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }
    public string PrintStatistics() 
    {
        var ret = string.Empty;

        if(_force)
        {
            ret = $"Found {DirectoriesFound} directories, deleted {DirectoriesDeleted} directories.";
        } 
        else
        {
            ret = $"Found {DirectoriesFound} directories, skipped deleting {DirectoriesDeleted} directories due to dry run.";
        }

        return ret;
    }
}