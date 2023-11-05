namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    internal interface IErrorCollection : IReadOnlyErrorCollection
    {
        void Add(IError error);
    }
}