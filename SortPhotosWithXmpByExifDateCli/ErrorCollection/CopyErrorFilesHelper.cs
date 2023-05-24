using System.Reflection.Metadata;
using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class CopyErrorFilesHelper
    {
        public static void CopyErrorFiles(this IReadOnlyErrorCollection errorCollection, ILogger logger)
        {
            var errors = errorCollection.Errors.OfType<FileAlreadyExistsError>().ToList();
            if (errors.Any())
            {
                var errorBaseDirectory = new DirectoryInfo("ErrorFiles");
                logger.LogError($"Could not copy all files due to collisions. Please check {errorBaseDirectory.FullName}");

                RenamePossiblyExistingErrorDirectory(errorBaseDirectory);
                CreateErrorDirectory(logger, errorBaseDirectory);

                logger.LogTrace($"Copy {errors.Count} files to {errorBaseDirectory.FullName}");
                foreach (var error in errors)
                {
                    logger.LogTrace($"Handling collision between {error.FileInfo} and {error.OtherFile}!");

                    // if we have a collision, we create a directory and copy all collisions with appended number into it
                    // A collision happens when we have several files with the same name and the same target directory
                    // All possible collision cases are captured by FileAlreadyExistsErrors:
                    // lets assume we have the images a/1.jpg, b/1.jpg, c/1.jpg that shall all get moved into the directory 20230101
                    // processing a/1.jpg: move a/1.jpg to 20230101/1.jpg
                    // processing b/1.jpg: moving b/1.jpg to 20230101/1.jpg fails with FileAlreadyExistsError("20230101/1.jpg", "b/1.jpg")
                    // processing c/1.jpg: moving c/1.jpg to 20230101/1.jpg fails with FileAlreadyExistsError("20230101/1.jpg", "c/1.jpg")
                    // handling the errors (this function) will receive 2 FileAlreadyExistsErrors and process them
                    // - process 1st FileAlreadyExistsError: 
                    //  * create collision directory ErrorFiles/20230101
                    //  * move error1.FileInfo ("20230101/1.jpg") to ErrorFiles/20230101/1.jpg
                    //  * copy error1.OtherFile with appended number to ErrorFiles/20230101/1_1.jpg
                    // - process 2nd FileAlreadyExistsError: 
                    //  * skip creating collision directory ErrorFiles/20230101
                    //  * skip moving error2.FileInfo ("20230101/1.jpg") to ErrorFiles/20230101/1.jpg
                    //  * copy error2.OtherFile with appended number to ErrorFiles/20230101/1_2.jpg

                    string filenameWithExtension = error.FileInfo.Name;
                    var (filename, extension) = SplitFileNameAndExtension(filenameWithExtension);
                    var collisionDirectoryInfo = new DirectoryInfo(Path.Combine(errorBaseDirectory.FullName, filename));
                    var collisionFileInfo = new FileInfo(Path.Combine(errorBaseDirectory.FullName, filenameWithExtension));

                    // 1: make sure directory exists
                    CreateCollisionDirectory(logger, collisionDirectoryInfo, collisionFileInfo);

                    // 2: move the already copied file to the other duplicates s.t. we can investigate easily
                    MoveBack(logger, error, collisionDirectoryInfo, collisionFileInfo);

                    if (collisionFileInfo.Exists)
                    {
                        throw new InvalidOperationException("3: we have the file already copied to the ErrorFiles, so move it into the subdirectory");
                    }

                    // 3: copy the other file into subdirectory with appended _number
                    CopyFileWithAppendedNumber(logger, error.OtherFile, collisionFileInfo, filename, extension);
                }
            }
        }

        private static void MoveBack(ILogger logger, FileAlreadyExistsError fileAlreadyExistsError, DirectoryInfo collisionDirectoryInfo, FileInfo collisionFileInfo)
        {
            var targetPath = Path.Combine(collisionDirectoryInfo.FullName, collisionFileInfo.Name);

            if (fileAlreadyExistsError.FileInfo.Exists)
            {
                logger.LogTrace($"Moving back {fileAlreadyExistsError.OtherFile.FullName} to where we store all the collisions: {targetPath}");
                File.Move(fileAlreadyExistsError.FileInfo.FullName, targetPath);
            }
            else
            {
                logger.LogTrace($"{fileAlreadyExistsError.OtherFile.FullName} has already been moved to {targetPath}");
            }
        }

        private static void CreateCollisionDirectory(ILogger logger, DirectoryInfo collisionDirectoryInfo, FileInfo collisionFileInfo)
        {
            if (!collisionDirectoryInfo.Exists)
            {
                logger.LogTrace($"Creating {collisionFileInfo.FullName}");
                collisionDirectoryInfo.Create();
            }
        }

        private static void CreateErrorDirectory(ILogger logger, DirectoryInfo errorBaseDirectory)
        {
            if (!Directory.Exists(errorBaseDirectory.FullName))
            {
                logger.LogTrace($"Creating {errorBaseDirectory.FullName}");
                Directory.CreateDirectory(errorBaseDirectory.FullName);
            }
        }

        private static void RenamePossiblyExistingErrorDirectory(DirectoryInfo errorBaseDirectory)
        {
            // rename possibly existing ErrorFiles directory (add lastWriteTime to the end)
            if (errorBaseDirectory.Exists)
            {
                var time = File.GetLastWriteTime(errorBaseDirectory.FullName).ToString("yyyyMMddTHHmmss");
                var parentDirectory = errorBaseDirectory.Parent ?? throw new InvalidOperationException("Path does not exist");
                var directoryName = errorBaseDirectory.Name;
                var newName = Path.Combine(parentDirectory.FullName, directoryName + "_" + time);
                Directory.Move(errorBaseDirectory.FullName, newName);
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
            Helpers.Copy(errorFileInfo.FullName, fullname);
        }
    }
}