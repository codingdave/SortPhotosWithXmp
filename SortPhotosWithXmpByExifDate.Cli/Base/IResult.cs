using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public interface IResult
{
    void Log();
    IReadOnlyErrorCollection ErrorCollection { get; }
    IReadOnlySuccessCollection SuccessfulCollection { get; }
}