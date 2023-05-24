namespace SortPhotosWithXmpByExifDateCli.Statistics;

public sealed class NoTimeFoundError : ErrorBase
{
    public NoTimeFoundError(FileInfo fileInfo, List<string> messages)
    : base(fileInfo, messages.Prepend(nameof(NoTimeFoundError)))
    {
    }
}
