namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public interface IError
    {
        FileInfo FileInfo { get; }
        string ErrorMessage { get; }
        string Name { get; }

        void AddMessage(string errorMessage);
    }
}