namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class IReadOnlyErrorCollectionHelpers
    {
        public static void CopyErrorFiles(this IReadOnlyFileError errorCollection, DirectoryInfo errorDirectory)
        {
            foreach (var error in errorCollection.Errors)
            {
                File.Copy(error.FileInfo.FullName, Path.Join(errorDirectory.FullName, error.FileInfo.Name), true);
            }
        }
    }
}