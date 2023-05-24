namespace SortPhotosWithXmpByExifDateCli.Statistics;

public abstract class ErrorBase : IError
{
    public ErrorBase(FileInfo fileInfo, IEnumerable<string> messages)
    {
        FileInfo = fileInfo;
        _messages = messages;
    }

    private IEnumerable<string> _messages;

    public string ErrorMessage => string.Join(Environment.NewLine, _messages);

    public bool HasErrors => true;

    public FileInfo FileInfo { get; }

    public void AddMessage(string errorMessage)
    {
        _messages = _messages.Append(errorMessage);
    }
}
