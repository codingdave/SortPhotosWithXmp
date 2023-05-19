namespace SortPhotosWithXmpByExifDateCli.Statistics
{

    public interface IReadOnlyFileError
    {
        public IReadOnlyList<IError> Errors { get; }
    }
}