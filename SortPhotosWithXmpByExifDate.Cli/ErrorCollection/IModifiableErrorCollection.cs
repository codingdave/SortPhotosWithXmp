namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    internal interface IModifiableErrorCollection
    {
        public void AddError(IError error);
    }
}