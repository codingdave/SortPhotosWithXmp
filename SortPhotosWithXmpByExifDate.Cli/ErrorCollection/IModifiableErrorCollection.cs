namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface IModifiableErrorCollection
    {
        public void AddError(IError error);
    }
}