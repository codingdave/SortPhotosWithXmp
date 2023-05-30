namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates.Store
{
    public record struct ImageHashDto(string ImagePath, ulong Hash, DateTime LastWriteTimeUtc);
}