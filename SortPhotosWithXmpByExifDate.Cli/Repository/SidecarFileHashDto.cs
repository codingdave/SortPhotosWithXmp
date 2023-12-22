namespace SortPhotosWithXmpByExifDate.Repository;

public record struct SidecarFileHashDto(string Filename, byte[] Hash, DateTime LastWriteTimeUtc) : IImageFileDto, IHashDto;
