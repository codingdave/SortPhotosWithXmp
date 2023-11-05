namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public record struct ImageFile(string Filename, DateTime LastWriteTimeUtc) : IImageFile
{
    public ImageFile(string filename) : this(filename, File.GetLastWriteTimeUtc(filename)) { }
}
