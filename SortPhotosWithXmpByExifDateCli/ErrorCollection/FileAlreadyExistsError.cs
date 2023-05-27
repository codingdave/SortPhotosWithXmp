namespace SortPhotosWithXmpByExifDateCli.Statistics;

public sealed class FileAlreadyExistsError : ErrorBase
{
    public FileAlreadyExistsError(FileInfo fileInfo, FileInfo otherFile, string message)
    : base(fileInfo, new List<string> { message })
    {
        OtherFile = otherFile;
    }

    public FileInfo OtherFile { get; }

    public override string Name => nameof(FileAlreadyExistsError);
}
