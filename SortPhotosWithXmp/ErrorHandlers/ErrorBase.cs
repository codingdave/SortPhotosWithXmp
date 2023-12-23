namespace SortPhotosWithXmp.ErrorHandlers;

public abstract class ErrorBase : IError
{
    public ErrorBase(string fileName, IEnumerable<string> messages)
    {
        FileName = fileName;
        _messages = messages;
    }

    private IEnumerable<string> _messages;

    public string ErrorMessage => string.Join(Environment.NewLine, _messages);

    public string FileName { get; }

    public abstract string Name { get; }

    public void AddMessage(string errorMessage)
    {
        _messages = _messages.Append(errorMessage);
    }

    public override string ToString()
    {
        return $"{FileName}. {ErrorMessage}";
    }
}
