using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class DeleteFilesStatistics : IStatistics
{
    public IReadOnlyErrorCollection FileErrors => throw new NotImplementedException();

    public void Log()
    {
        throw new NotImplementedException();
    }
}
