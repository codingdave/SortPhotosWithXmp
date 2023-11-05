namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public interface IImageFile
{
    string Filename { get; }
    DateTime LastWriteTimeUtc { get; }
    public bool IsModified => LastWriteTimeUtc != File.GetLastWriteTime(Filename);
}
