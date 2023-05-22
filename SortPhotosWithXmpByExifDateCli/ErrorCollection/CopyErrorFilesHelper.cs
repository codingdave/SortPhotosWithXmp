using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class CopyErrorFilesHelper
    {
        public static void CopyErrorFiles(this IReadOnlyFileError errorCollection, ILogger logger)
        {
            if (errorCollection.Errors.Any())
            {
                var distincsErrorFiles = errorCollection.Errors.Select(e => e.FileInfo).Distinct();
                var errorBaseDirectory = new DirectoryInfo("../ErrorFiles");

                if (errorBaseDirectory.Exists)
                {
                    var time = File.GetLastWriteTime(errorBaseDirectory.FullName).ToString("yyyyMMddTHHmmss");
                    var p = errorBaseDirectory.Parent;
                    var n = errorBaseDirectory.Name;
                    var newName = Path.Combine(p.FullName, n + "_" + time);
                    Directory.Move(errorBaseDirectory.FullName, newName);
                }

                logger.LogInformation($"Copy {distincsErrorFiles.Count()} files to {errorBaseDirectory.FullName}");
                foreach (var errorFileInfo in distincsErrorFiles)
                {
                    var fileDirectory = Path.Combine(errorBaseDirectory.FullName);
                    if (!Directory.Exists(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    File.Copy(errorFileInfo.FullName, Path.Join(fileDirectory, errorFileInfo.Name));
                }
            }
        }
    }
}