namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class MultipleErrors : IError
{
    readonly IEnumerable<string> _messages;

    public MultipleErrors(FileInfo fileInfo, IEnumerable<string> messages)
    {
        FileInfo = fileInfo;
        _messages = messages;
    }

    public string ErrorMessage => FileInfo.FullName + ":" + Environment.NewLine + String.Join(Environment.NewLine, _messages);

    public bool HasErrors => true;

    public FileInfo FileInfo { get; }
}