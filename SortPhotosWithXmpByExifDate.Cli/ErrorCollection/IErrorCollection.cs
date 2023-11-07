namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface IErrorCollection : IReadOnlyErrorCollection
    {
        void Add(IError error);
    }
}