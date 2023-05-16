namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class IReadOnlyErrorCollectionHelpers
    {
        public static void CopyErrorFiles(this IReadOnlyErrorCollection errorCollection, DirectoryInfo errorDirectory)
        {
            foreach (var (_, file) in errorCollection.Errors)
            {
                File.Copy(file.FullName, Path.Join(errorDirectory.FullName, file.Name), true);
            }
        }
    }
}