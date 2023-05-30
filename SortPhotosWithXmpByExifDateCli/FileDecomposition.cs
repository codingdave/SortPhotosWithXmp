namespace SortPhotosWithXmpByExifDateCli
{
    public static partial class CopyErrorFilesHelper
    {
        public readonly record struct FileDecomposition(string CompletePath, string Directory, string Name, string Extension);
    
    }
}