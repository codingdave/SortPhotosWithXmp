namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public interface IReadOnlyErrorCollection
    {
        public IReadOnlyList<(string message, FileInfo file)> Errors { get; }
    }
}