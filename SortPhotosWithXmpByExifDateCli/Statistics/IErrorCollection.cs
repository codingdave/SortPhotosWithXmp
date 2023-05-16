namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IErrorCollection : IReadOnlyErrorCollection
{
    void Add((string, FileInfo f) value);
    void AddRange(List<(string message, FileInfo fileInfo)> list);
}
