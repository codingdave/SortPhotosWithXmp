namespace SortPhotosWithXmpByExifDateCli.Statistics;

public interface IFileError : IReadOnlyFileError
{
    void Add(IError error);
}
