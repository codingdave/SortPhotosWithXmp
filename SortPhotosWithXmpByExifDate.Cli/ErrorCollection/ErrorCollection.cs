using System.Collections.Concurrent;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class ErrorCollection<T> : IErrorCollection<T> where T : IError
{
    public ErrorCollection() { }

    public IEnumerable<T> Errors => _errors.ToList();

    private readonly ConcurrentBag<T> _errors = new();

    public void Add(T error)
    {
        var existingError = _errors.FirstOrDefault(e => string.Equals(e.File, error.File));
        if (existingError == null)
        {
            _errors.Add(error);
        }
        else
        {
            existingError.AddMessage(error.ErrorMessage);
        }
    }
}