namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates.Store
{
    public record struct XmpHashDto(string Filename, byte[] Hash, DateTime LastWriteTimeUtc): IHashDto;
}