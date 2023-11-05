namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public record struct ImageFileHashDto(string Filename, ulong Hash, DateTime LastWriteTimeUtc) : IImageFileDto, IPerceptualHashDto;

