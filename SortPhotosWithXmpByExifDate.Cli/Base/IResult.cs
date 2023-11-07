using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public interface IResult
{
    void Log();
    IReadOnlyErrorCollection ErrorCollection { get; }
    IReadOnlySuccessCollection SuccessfulCollection { get; }
}