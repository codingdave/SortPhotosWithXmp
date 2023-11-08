using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class ErrorCollection : IErrorCollection, IReadOnlyErrorCollection
{
    public ErrorCollection() { }

    public IReadOnlyList<IError> Errors => _errors.ToList();
    private readonly ConcurrentBag<IError> _errors = new();

    public void Add(IError error)
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