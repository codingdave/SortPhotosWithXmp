namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class DirectoriesDeletedStatistics : IStatistics, IErrors
{
    private bool _force;
    public DirectoriesDeletedStatistics(bool force)
    {
        _force = force;
    }

    public IErrorCollection ErrorCollection { get; } = new ErrorCollection();
    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }
    public string PrintStatistics() 
    {
        var ret = $"Found {DirectoriesFound} directories";

        if(DirectoriesDeleted > 0 && _force)
        {
            ret += $", deleted {DirectoriesDeleted} directories";
        } 
        else
        {
            ret += $", skipped deleting {DirectoriesDeleted} directories due to dry run";
        }

        ret += string.Join(System.Environment.NewLine, ErrorCollection.Errors);

        return ret;
    }
}