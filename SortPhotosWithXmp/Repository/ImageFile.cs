namespace SortPhotosWithXmp.Repository;

public record struct ImageFile(string OriginalFilename, DateTime LastWriteTimeUtc) : IImageFile
{
    #warning unmockable FILE
    #warning Update time when setting NewFilename?
    public ImageFile(string originalFilename) : this(originalFilename, File.GetLastWriteTimeUtc(originalFilename)) { }
    public string? NewFilename { get; set; } = null;
    public string CurrentFilename => NewFilename ?? OriginalFilename;
}
