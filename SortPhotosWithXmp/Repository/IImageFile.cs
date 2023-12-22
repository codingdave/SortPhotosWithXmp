namespace SortPhotosWithXmp.Repository;

public interface IImageFile
{
    string OriginalFilename { get; }
    DateTime LastWriteTimeUtc { get; }
    public bool IsModified => LastWriteTimeUtc != File.GetLastWriteTime(CurrentFilename);
    public string? NewFilename { get; set; }
    public string CurrentFilename { get; }
}
