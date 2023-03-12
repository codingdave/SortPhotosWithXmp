namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ErrorCollection : IErrorCollection
{
    public List<string> Errors { get; } = new List<string>();
} 
