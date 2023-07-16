namespace SortPhotosWithXmpByExifDateCli.Repository;

public record struct ImageFile(string Filename, DateTime LastWriteTimeUtc) : IImageFile
{
    public ImageFile(string Filename) : this(Filename, File.GetLastWriteTimeUtc(Filename)) { }
}
