namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates.Store
{
    public record struct ImageHashDto(string Filename, ulong Hash, DateTime LastWriteTimeUtc): IHashDto;
}