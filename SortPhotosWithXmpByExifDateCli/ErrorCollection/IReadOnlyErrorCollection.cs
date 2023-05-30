namespace SortPhotosWithXmpByExifDateCli.Statistics
{

    public interface IReadOnlyErrorCollection
    {
        public IReadOnlyList<IError> Errors { get; }
    }
}