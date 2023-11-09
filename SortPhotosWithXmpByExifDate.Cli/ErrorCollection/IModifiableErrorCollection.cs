namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface IModifiableErrorCollection<T> where T : IError
    {
        public void AddError(T error);
    }
}