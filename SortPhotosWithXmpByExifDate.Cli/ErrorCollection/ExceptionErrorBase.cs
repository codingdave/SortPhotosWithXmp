namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public abstract class ExceptionErrorBase : ErrorBase
{
    public ExceptionErrorBase(string file, Exception exception, List<string> messages)
    : base(file, messages) => Exception = exception;

    public ExceptionErrorBase(string file, Exception exception)
    : this(file, exception, new List<string>() { nameof(ExceptionErrorBase) + ": " + exception.Message })
    {
    }

    public Exception Exception { get; }

    public override string Name => nameof(ExceptionErrorBase);
}
