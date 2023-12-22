namespace SortPhotosWithXmpByExifDate.Repository;

public record struct ImageFileHash(string OriginalFilename, ulong Hash, DateTime LastWriteTimeUtc) : IImageFile, IPerceptualHash
{
    public string? NewFilename { get; set; } = null;
    public string CurrentFilename => NewFilename ?? OriginalFilename;
}


