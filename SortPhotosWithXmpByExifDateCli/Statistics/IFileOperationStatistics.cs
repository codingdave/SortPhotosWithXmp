using SortPhotosWithXmpByExifDateCli.Operation;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public interface IFileOperationStatistics
    {
        IFileOperation FileOperation { get; }
    }
}