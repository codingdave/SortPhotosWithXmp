using SortPhotosWithXmpByExifDate.Cli.Operations;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics
{
    public interface IFileOperationStatistics
    {
        IFileOperation FileOperation { get; }
    }
}