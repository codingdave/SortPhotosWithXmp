namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    public interface IHash
    {
        public string Filename { get; }
        DateTime LastWriteTimeUtc { get; }
    }
}