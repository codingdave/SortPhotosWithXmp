using System.Diagnostics;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class FileError : IFileError
{
    public IReadOnlyList<IError> Errors => _errors;
    private readonly List<IError> _errors = new();

    public void Add(IError error)
    {
        _errors.Add(error);
    }
}