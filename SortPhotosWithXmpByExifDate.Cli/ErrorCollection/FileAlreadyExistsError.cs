namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public sealed class FileAlreadyExistsError : ErrorBase
{
    public FileAlreadyExistsError(string file, string otherFile, string message)
    : base(file, new List<string> { message })
    {
        OtherFile = otherFile;
    }

    public string OtherFile { get; }

    public override string Name => nameof(FileAlreadyExistsError);
}
