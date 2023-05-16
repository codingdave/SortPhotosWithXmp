using System.Net.Http.Headers;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ErrorCollection : IErrorCollection
{
    public IReadOnlyList<(string message, FileInfo file)> Errors => _errors;
    private readonly List<(string message, FileInfo file)> _errors = new();

    public void Add((string, FileInfo f) value)
    {
        _errors.Add(value);
    }

    public void AddRange(List<(string message, FileInfo fileInfo)> list)
    {
        _errors.AddRange(list);
    }
}