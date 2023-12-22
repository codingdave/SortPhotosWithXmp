namespace SortPhotosWithXmp.Repository;

public record struct SidecarFileHash(string OriginalFilename, byte[] Hash, DateTime LastWriteTimeUtc) : IImageFile, IHash
{
    public string? NewFilename { get; set; } = null;
    public string CurrentFilename => NewFilename ?? OriginalFilename;
}

