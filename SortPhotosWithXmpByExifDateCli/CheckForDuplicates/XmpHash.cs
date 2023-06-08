namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    public record struct XmpHash(string Filename, byte[] Hash, DateTime LastWriteTimeUtc) : HashBase;
}