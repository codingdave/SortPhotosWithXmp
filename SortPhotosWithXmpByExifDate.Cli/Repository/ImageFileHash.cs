namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public record struct ImageFileHash(string Filename, ulong Hash, DateTime LastWriteTimeUtc) : IImageFile, IPerceptualHash;

