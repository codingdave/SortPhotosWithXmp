namespace SortPhotosWithXmpByExifDateCli.Entities
{
    public record struct XmpHashDto(string Filename, byte[] Hash, DateTime LastWriteTimeUtc) : IHashDto;
}