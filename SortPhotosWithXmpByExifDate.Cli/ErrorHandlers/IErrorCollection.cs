namespace SortPhotosWithXmpByExifDate.ErrorHandlers
{
    public interface IErrorCollection<T> where T : IError
    {
        void Add(T error);

        IEnumerable<T> Errors { get; }
    }
}