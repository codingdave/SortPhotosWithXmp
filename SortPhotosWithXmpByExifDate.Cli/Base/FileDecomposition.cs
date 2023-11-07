namespace SortPhotosWithXmpByExifDate.Cli
{
    public readonly record struct FileDecomposition(string CompletePath, string Directory, string Name, string Extension);
}