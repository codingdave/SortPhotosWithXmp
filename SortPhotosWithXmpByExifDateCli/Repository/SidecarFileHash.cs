namespace SortPhotosWithXmpByExifDateCli.Repository;

public record struct SidecarFileHash(string Filename, byte[] Hash, DateTime LastWriteTimeUtc) : IImageFile, IHash;
