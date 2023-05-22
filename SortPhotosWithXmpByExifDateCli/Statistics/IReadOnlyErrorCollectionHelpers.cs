namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class IReadOnlyErrorCollectionHelpers
    {
        public static void CopyErrorFiles(this IReadOnlyFileError errorCollection)
        {
            if (errorCollection.Errors.Any())
            {
                var errorBaseDirectory = new DirectoryInfo("ErrorFiles");
                Console.WriteLine($"Copy {errorCollection.Errors.Count} files to {errorBaseDirectory.FullName}");
                foreach (var error in errorCollection.Errors)
                {
                    var fileDirectory = Path.Combine(errorBaseDirectory.FullName);
                    if (!Directory.Exists(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    File.Copy(error.FileInfo.FullName, Path.Join(fileDirectory, error.FileInfo.Name), true);
                }
            }
        }
    }
}