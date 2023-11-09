namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface IErrorCollection<T> where T : IError
    {
        void Add(T error);

        IEnumerable<T> Errors { get; }
    }
}