using Microsoft.Extensions.Logging;
using ImageMagick;
using System.Diagnostics;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class CopyErrorFilesHelper
    {
        public static void HandleErrorFiles(this IReadOnlyErrorCollection errorCollection, ILogger logger, IFoundStatistics statistics)
        {
            var copyFileOperation = new CopyFileOperation(logger, statistics.FileOperation.IsChanging);
            var deleteFileOperation = new DeleteFileOperation(logger, statistics.FileOperation.IsChanging);

            CollectCollisions(logger, errorCollection.Errors.OfType<FileAlreadyExistsError>(), (string baseDirectory, FileAlreadyExistsError error) => HandleFileAlreadyExistsError(baseDirectory, error, logger, statistics, copyFileOperation, deleteFileOperation));
            CollectCollisions(logger, errorCollection.Errors.OfType<NoTimeFoundError>(), (string baseDirectory, NoTimeFoundError error) => HandleCollisions(baseDirectory, error, logger, copyFileOperation));
            CollectCollisions(logger, errorCollection.Errors.OfType<MetaDataError>(), (string baseDirectory, MetaDataError error) => HandleCollisions(baseDirectory, error, logger, copyFileOperation));
        }

        private static void HandleCollisions<T>(string baseDirectory, T error, ILogger logger, CopyFileOperation copyFileOperation) where T : ErrorBase
        {
            var (completeFilePath, directory, _, _) = DecomposeFile(baseDirectory, error.File);
            CreateDirectoryAndCopyFile(logger, completeFilePath, directory, error, copyFileOperation);
        }

        private static void HandleFileAlreadyExistsError(string baseDirectory, FileAlreadyExistsError error, ILogger logger, IFoundStatistics statistics, CopyFileOperation copyFileOperation, DeleteFileOperation deleteFileOperation)
        {
            HandleCollisionOrDuplicate(logger, statistics, copyFileOperation, deleteFileOperation, baseDirectory, error);
        }

        private static void CollectCollisions<T>(ILogger logger,
                                              IEnumerable<T> errors,
                                              Action<string, T> action) where T : ErrorBase
        {
            if (errors.Any())
            {
                var directoryName = errors.First().Name;
                var baseDirectory = new DirectoryInfo(directoryName).FullName;
                logger.LogError($"{errors.Count()} {directoryName} issues will be located in the directory '{baseDirectory}'");

                RenamePossiblyExistingDirectory(logger, baseDirectory);
                CreateDirectory(logger, baseDirectory);

                foreach (var error in errors)
                {
                    try
                    {
                        action(baseDirectory, error);
                    }
                    catch (Exception e)
                    {
                        logger.LogExceptionError(e);
                    }
                }
            }
        }


        private static (string completeFilepath, string directory, string filename, string extension) DecomposeFile(
            string baseDirectory,
            string errorFile)
        {
            string filenameWithExtension = Path.GetFileName(errorFile);
            var (filename, extension) = SplitFileNameAndExtension(filenameWithExtension);
            var directory = Path.Combine(baseDirectory, filename);
            var completeFilepath = Path.Combine(baseDirectory, filenameWithExtension);

            Serilog.Log.Verbose($"basedirectory: {baseDirectory}, errorFile: {errorFile} => completeFilepath: {completeFilepath}, directory: {directory}, filename: {filename}, extension: {extension}");

            return (completeFilepath, directory, filename, extension);
        }

        private static void CreateDirectoryAndCopyFile(ILogger logger, string file, string directory, ErrorBase error, CopyFileOperation copyFileOperation)
        {
            // 1: make sure directory exists
            CreateDirectory(logger, directory);

            // 2: copy the first file of the collision to the other duplicates s.t. we can investigate easily
            Copy(error, directory, file, copyFileOperation);
        }

        private static void HandleCollisionOrDuplicate(ILogger logger,
                                                       IFoundStatistics statistics,
                                                       CopyFileOperation copyFileOperation,
                                                       DeleteFileOperation deleteFileOperation,
                                                       string baseDirectory,
                                                       FileAlreadyExistsError error)
        {
            if (IsDuplicate(logger, error, statistics))
            {
                logger.LogDebug($"{error.OtherFile} is duplicate of {error.File}");
                deleteFileOperation.Delete(error.OtherFile);
            }
            else
            {
                HandleCollision(logger, baseDirectory, error, copyFileOperation);
            }
        }

        private static bool IsDuplicate(ILogger logger,
                                        FileAlreadyExistsError error,
                                        IFoundStatistics statistics)
        {
            // when are 2 images identical?
            var isDuplicate = false;

            // same type (image vs xmp)
            var extensionFile = Path.GetExtension(error.File);
            var extensionOther = Path.GetExtension(error.OtherFile);
            var sameExtension = extensionFile == extensionOther;
            Debug.Assert(sameExtension, "Extension should always match");

            if (sameExtension)
            {
                if (extensionFile.EndsWith("xmp", StringComparison.OrdinalIgnoreCase))
                {
                    isDuplicate = AreXmpsDuplicates(error, statistics);
                }
                else
                {
                    isDuplicate = AreImagesDuplicates(logger, error, statistics);
                }
            }

            return isDuplicate;
        }

        private static bool AreXmpsDuplicates(FileAlreadyExistsError error,
                                              IFoundStatistics statistics)
        {
            // xmps are identical, if their hash is identical

            using var md5 = System.Security.Cryptography.MD5.Create();
            using var fileStream = File.OpenRead(error.File);
            using var otherfileStream = File.OpenRead(error.OtherFile);
            var hash1 = md5.ComputeHash(fileStream);
            var hash2 = md5.ComputeHash(otherfileStream);
            bool isHashIdentical = hash1 == hash2;
            if (isHashIdentical)
            {
                statistics.SkippedXmps++;
            }

            return isHashIdentical;
        }

        private static bool AreImagesDuplicates(ILogger logger,
                                                FileAlreadyExistsError error,
                                                IFoundStatistics statistics)
        {
            bool isDuplicate = false;

            try
            {
                ResourceLimits.LimitMemory(new Percentage(90));
                using var copiedImage = new MagickImage(error.File);
                using var otherImage = new MagickImage(error.OtherFile);
                var distortion = copiedImage.Compare(otherImage, ErrorMetric.Absolute);
                var isDistorted = distortion > .000001;

                isDuplicate = !isDistorted;

                if (isDuplicate)
                {
                    statistics.SkippedImages++;
                }
            }
            catch (Exception e)
            {
                logger.LogExceptionError(e);
            }

            return isDuplicate;
        }

        private static void HandleCollision(ILogger logger,
                                            string baseDirectory,
                                            FileAlreadyExistsError error,
                                            CopyFileOperation copyFileOperation)
        {
            logger.LogTrace($"Handling collision between {error.File} and {error.OtherFile}!");

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
            //  * copy error1.FileInfo ("20230101/1.jpg") to ErrorFiles/20230101/1.jpg
            //  * copy error1.OtherFile with appended number to ErrorFiles/20230101/1_1.jpg
            // - process 2nd FileAlreadyExistsError: 
            //  * skip creating collision directory ErrorFiles/20230101
            //  * skip copying error2.FileInfo ("20230101/1.jpg") to ErrorFiles/20230101/1.jpg
            //  * copy error2.OtherFile with appended number to ErrorFiles/20230101/1_2.jpg


            var ( completeFilepath, directory, filename, extension) = DecomposeFile(baseDirectory, error.File);

            CreateDirectoryAndCopyFile(logger, completeFilepath, directory, error, copyFileOperation);

            // 3: copy the other file into subdirectory with appended _number
            CopyFileWithAppendedNumber(logger, error.OtherFile, completeFilepath, filename, extension, copyFileOperation);
        }

        private static void Copy(ErrorBase error,
                                 string directory,
                                 string file,
                                 CopyFileOperation fileOperation)
        {
            var targetPath = Path.Combine(directory, Path.GetFileName(file));

            if (File.Exists(error.File))
            {
                fileOperation.ChangeFile(error.File, targetPath);
            }
            else
            {
                throw new FileNotFoundException($"'{error.File}' does not exist.");
            }
        }

        private static void CreateDirectory(ILogger logger, string directory)
        {
            if (!Directory.Exists(directory))
            {
                logger.LogTrace("Creating directory '{newDirectory}'", directory);
                Directory.CreateDirectory(directory);
            }
        }

        private static void RenamePossiblyExistingDirectory(ILogger logger, string directory)
        {
            // rename possibly existing ErrorFiles directory (add lastWriteTime to the end)
            if (Directory.Exists(directory))
            {
                var time = File.GetLastWriteTime(directory).ToString("yyyyMMddTHHmmss");
                var d = new DirectoryInfo(directory);
                var parentDirectory = d.Parent ?? throw new InvalidOperationException("Path does not exist");
                var directoryName = d.Name;
                var newName = Path.Combine(parentDirectory.FullName, directoryName + "_" + time);
                logger.LogTrace("Renaming {oldDirectory} to {newDirectory}", directory, newName);
                Directory.Move(directory, newName);
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

        private static void CopyFileWithAppendedNumber(ILogger logger,
                                                       string errorFile,
                                                       string collisionFile,
                                                       string filename,
                                                       string extension,
                                                       CopyFileOperation copyFileOperation)
        {
            // copy this file into subdirectory with appended _number
            var directory = new FileInfo(collisionFile).Directory ?? throw new InvalidOperationException("Directory does not exist");
            var subdirectory = Path.Combine(directory.FullName, filename);
            var fileCount = Directory.GetFiles(subdirectory, "*" + extension).Length;
            var fullname = Path.Combine(subdirectory, filename + "_" + fileCount + extension);
            logger.LogDebug("Collision for {errorFileInfo}. Arrange next to others as {fullname}", errorFile, fullname);
            copyFileOperation.ChangeFile(errorFile, fullname);
        }
    }
}