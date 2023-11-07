namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public interface IFileOperationStatistics
    {
        ICopyOrMoveFileOperation FileOperation { get; }
    }
}