namespace SortPhotosWithXmp.ErrorHandlers;

public abstract class ErrorBase : IError
{
    public ErrorBase(string file, IEnumerable<string> messages)
    {
        File = file;
        _messages = messages;
    }

    private IEnumerable<string> _messages;

    public string ErrorMessage => string.Join(Environment.NewLine, _messages);

    public string File { get; }

    public abstract string Name { get; }

    public void AddMessage(string errorMessage)
    {
        _messages = _messages.Append(errorMessage);
    }

    public override string ToString()
    {
        return $"{File}. {ErrorMessage}";
    }
}
