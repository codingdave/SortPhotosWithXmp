namespace SortPhotosWithXmpByExifDateCli.Operation
{
    public interface IFileOperation : IOperation
    {
        void ChangeFile(string sourceFileName, string destFileName);
    }
}