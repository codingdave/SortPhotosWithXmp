namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    public record struct ImageHash(string ImagePath, ulong Hash, DateTime LastWriteTimeUtc);
}