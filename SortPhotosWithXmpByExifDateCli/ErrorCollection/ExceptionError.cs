namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ExceptionError : ErrorBase
{
    public ExceptionError(string file, Exception exception, List<string> messages)
    : base(file, messages)
    {
        Exception = exception;
    }

    public ExceptionError(string file, Exception exception)
    : this(file, exception, new List<string>() { nameof(ExceptionError) + ": " + exception.Message })
    {
    }

    public Exception Exception { get; }

    public override string Name => nameof(ExceptionError);
}
