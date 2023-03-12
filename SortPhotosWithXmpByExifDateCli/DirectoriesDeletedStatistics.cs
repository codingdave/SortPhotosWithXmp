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
        var ret = $"Found {DirectoriesFound} directories";

        if(DirectoriesDeleted > 0 && _force)
        {
            ret = $", deleted {DirectoriesDeleted} directories";
        } 
        else
        {
            ret = $", skipped deleting {DirectoriesDeleted} directories due to dry run";
        }

        return ret;
    }
}