namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public interface IImageFile
{
    string OriginalFilename { get; }
    DateTime LastWriteTimeUtc { get; }
    public bool IsModified => LastWriteTimeUtc != File.GetLastWriteTime(OriginalFilename);
    public string? NewFilename { get; set; }
    public string CurrentFilename { get; }
}
