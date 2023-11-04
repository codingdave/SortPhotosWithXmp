namespace SortPhotosWithXmpByExifDateCli.ErrorCollection
{
    public interface IReadOnlyErrorCollection
    {
        public IReadOnlyList<IError> Errors { get; }
    }
}