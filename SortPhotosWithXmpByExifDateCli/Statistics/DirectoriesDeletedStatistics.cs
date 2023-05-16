using MetadataExtractor;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class DirectoriesDeletedStatistics : IStatistics
{
    private readonly bool _force;
    public DirectoriesDeletedStatistics(bool force)
    {
        _force = force;
    }

    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }

    public IReadOnlyErrorCollection ErrorCollection { get; } = new ErrorCollection();

    public string PrintStatistics()
    {
        var ret = $"Found {DirectoriesFound} directories";

        ret += DirectoriesDeleted switch
        {
            > 0 when _force => $", deleted {DirectoriesDeleted} directories",
            _ => $", skipped deleting {DirectoriesDeleted} directories due to dry run",
        };
        ret += string.Join(System.Environment.NewLine, ErrorCollection.Errors);

        return ret;
    }
}