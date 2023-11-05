namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public interface IFileOperation : IOperation
    {
        void ChangeFile(string sourceFileName, string destFileName);
    }
}