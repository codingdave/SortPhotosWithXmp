namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public record struct ImageFileDto(string Filename, DateTime LastWriteTimeUtc) : IImageFileDto;