namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ExceptionError : ErrorBase
{
    public ExceptionError(FileInfo fileInfo, Exception exception, List<string> messages)
    : base(fileInfo, messages)
    {
        Exception = exception;
    }

    public ExceptionError(FileInfo fileInfo, Exception exception)
    : this(fileInfo, exception, new List<string>() { nameof(ExceptionError) + ": " + exception.Message })
    {
    }

    public Exception Exception { get; }

    public override string Name => nameof(ExceptionError);
}
