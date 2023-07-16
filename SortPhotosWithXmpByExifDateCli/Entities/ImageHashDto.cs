namespace SortPhotosWithXmpByExifDateCli.Entities
{
    public record struct ImageHashDto(string Filename, ulong Hash, DateTime LastWriteTimeUtc) : IHashDto;
}