namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public interface IError
    {
        FileInfo FileInfo { get; }
        string ErrorMessage { get; }
        bool HasErrors { get; }
    }
}