using SortPhotosWithXmpByExifDateCli.Operations;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public interface IFileOperationStatistics
    {
        IFileOperation FileOperation { get; }
    }
}