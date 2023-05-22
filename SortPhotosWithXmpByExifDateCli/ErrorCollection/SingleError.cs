namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class SingleError : IError
{
    readonly string _message;

    public SingleError(FileInfo fileInfo, string message)
    {
        FileInfo = fileInfo;
        _message = message;
    }

    public string ErrorMessage => _message;

    public bool HasErrors => true;

    public FileInfo FileInfo { get; }
}