namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    public record struct ImageHash(string Filename, ulong Hash, DateTime LastWriteTimeUtc) : HashBase;
}