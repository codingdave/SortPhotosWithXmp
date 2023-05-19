namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public class NoError : IError
    {
        public NoError(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

        public string ErrorMessage => string.Empty;

        public bool HasErrors => false;

        public FileInfo FileInfo { get; }
    }
}