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
                var errorBaseDirectory = new DirectoryInfo("ErrorFiles");
                logger.LogError($"Could not copy all files due to collisions. Please check {errorBaseDirectory.FullName}");

                if (errorBaseDirectory.Exists)
                {
                    var time = File.GetLastWriteTime(errorBaseDirectory.FullName).ToString("yyyyMMddTHHmmss");
                    var parentDirectory = errorBaseDirectory.Parent ?? throw new InvalidOperationException("Path does not exist");
                    var directoryName = errorBaseDirectory.Name;
                    var newName = Path.Combine(parentDirectory.FullName, directoryName + "_" + time);
                    Directory.Move(errorBaseDirectory.FullName, newName);
                }

                logger.LogTrace($"Copy {distincsErrorFiles.Count()} files to {errorBaseDirectory.FullName}");
                foreach (var errorFileInfo in distincsErrorFiles)
                {
                    if (!Directory.Exists(errorBaseDirectory.FullName))
                    {
                        logger.LogTrace($"Creating {errorBaseDirectory.FullName}");
                        Directory.CreateDirectory(errorBaseDirectory.FullName);
                    }

                    // if we have a collision, we create a directory and copy all collisions with appended number into it
                    // collision case 1) We already have directory, that means we have at least 2 items in it
                    // collision case 2) We are not having a directory but this file is already placed in the target directory 
                    
                    var (filename, extension) = SplitFileNameAndExtension(errorFileInfo.Name);
                    var collisionDirectoryInfo = new DirectoryInfo(Path.Combine(errorBaseDirectory.FullName, filename));
                    var collisionFileInfo = new FileInfo(Path.Combine(errorBaseDirectory.FullName, errorFileInfo.Name));

                    logger.LogTrace($"{collisionDirectoryInfo.FullName} Exists: {collisionDirectoryInfo.Exists}");
                    logger.LogTrace($"{collisionFileInfo.FullName} Exists: {collisionFileInfo.Exists}");
                    if (collisionDirectoryInfo.Exists || collisionFileInfo.Exists)
                    {
                        logger.LogTrace($"Collision detected for {collisionFileInfo}!");

                        // make sure directory exists
                        if (!collisionDirectoryInfo.Exists)
                        {
                            logger.LogTrace($"Creating {collisionFileInfo.FullName}");
                            collisionDirectoryInfo.Create();
                        }

                        // we have the file already copied, so move it into the subdirectory
                        if (collisionFileInfo.Exists)
                        {
                            var targetPath = Path.Combine(collisionDirectoryInfo.FullName, collisionFileInfo.Name);
                            logger.LogTrace($"Moving {collisionFileInfo.FullName} to {targetPath}");
                            File.Move(collisionFileInfo.FullName, targetPath);
                        }

                        // copy this file into subdirectory with appended _number
                        CopyFileWithAppendedNumber(logger, errorFileInfo, collisionFileInfo, filename, extension);
                    }
                    else
                    {
                        // if there is no collision, we are just copying
                        File.Copy(errorFileInfo.FullName, Path.Join(errorBaseDirectory.FullName, errorFileInfo.Name));
                    }
                }
            }
        }

        private static (string filename, string extension) SplitFileNameAndExtension(string name)
        {
            var ret = (string.Empty, string.Empty);
            var dotPosition = name.IndexOf('.');
            if (dotPosition > 0)
            {
                ret = (name[..dotPosition], name[dotPosition..]);
            }
            return ret;
        }

        private static void CopyFileWithAppendedNumber(ILogger logger, FileInfo errorFileInfo, FileInfo collisionFileInfo, string filename, string extension)
        {
            // copy this file into subdirectory with appended _number
            var directory = collisionFileInfo.Directory ?? throw new InvalidOperationException("Directory does not exist");
            var subdirectory = Path.Combine(directory.FullName, filename);
            var fileCount = Directory.GetFiles(subdirectory, "*" + extension).Length;
            var fullname = Path.Combine(subdirectory, filename + "_" + fileCount + extension);
            logger.LogDebug("Collision for {errorFileInfo}. Copy next to others as {fullname}", errorFileInfo, fullname);
            File.Copy(errorFileInfo.FullName, fullname);
        }
    }
}