using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public class DeleteFilesResult : IResult
{
    public DeleteFilesResult() => throw new NotImplementedException();

    public IReadOnlyErrorCollection ErrorCollection => throw new NotImplementedException();

    public IReadOnlySuccessCollection SuccessfulCollection => throw new NotImplementedException();

    public void Log()
    {
        throw new NotImplementedException();
    }
}
