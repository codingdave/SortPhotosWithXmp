namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    internal interface IErrorCollection : IReadOnlyErrorCollection
    {
        void Add(IError error);
    }
}