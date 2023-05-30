namespace SortPhotosWithXmpByExifDateCli.ErrorCollection
{
    internal interface IErrorCollection : IReadOnlyErrorCollection
    {
        void Add(IError error);
    }
}