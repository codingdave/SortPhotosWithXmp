namespace SortPhotosWithXmp.ErrorHandlers;

public class GeneralExceptionError : ExceptionErrorBase
{
    public GeneralExceptionError(string file, Exception exception) : base(file, exception)
    {
    }

    public GeneralExceptionError(string file, Exception exception, List<string> messages) : base(file, exception, messages)
    {
    }
}
