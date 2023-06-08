namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    public interface HashBase
    {
        public string Filename { get; }
        DateTime LastWriteTimeUtc { get; }
    }
}