using System.Reflection.Metadata;
using Microsoft.Extensions.Logging;
using ImageMagick;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    public static class CopyErrorFilesHelper
    {
        public static void HandleErrorFiles(this IReadOnlyErrorCollection errorCollection, ILogger logger, IFoundStatistics statistics)
        {
            CopyFileOperation copyFileOperation = new CopyFileOperation(logger, statistics.FileOperation.IsChanging);
            MoveFileOperation moveFileOperation = new MoveFileOperation(logger, statistics.FileOperation.IsChanging);

            CollectCollisions(logger, errorCollection.Errors.OfType<FileAlreadyExistsError>(), statistics, copyFileOperation, moveFileOperation, HandleFileAlreadyExistsError);
            CollectCollisions(logger, errorCollection.Errors.OfType<NoTimeFoundError>(), statistics, copyFileOperation, moveFileOperation, HandleCollisions);
            CollectCollisions(logger, errorCollection.Errors.OfType<MetaDataError>(), statistics, copyFileOperation, moveFileOperation, HandleCollisions);

            void HandleCollisions<T>(string baseDirectory, T error) where T : ErrorBase
            {
                CreateDirectoryAndMoveFile(logger, moveFileOperation, baseDirectory, error);
            }

            void HandleFileAlreadyExistsError(string baseDirectory, FileAlreadyExistsError error)
            {
                var deleteFileOperation = new DeleteFileOperation(logger, copyFileOperation.IsChanging);
                HandleCollisionOrDuplicate(logger, statistics, copyFileOperation, moveFileOperation, deleteFileOperation, baseDirectory, error);
            }
        }

        private static void CollectCollisions<T>(ILogger logger,
                                              IEnumerable<T> errors,
                                              IFoundStatistics statistics,
                                              CopyFileOperation copyFileOperation,
                                              MoveFileOperation moveFileOperation,
                                              Action<string, T> action) where T : ErrorBase
        {
            if (errors.Any())
            {
                var directoryName = errors.First().Name;
                var baseDirectory = directoryName;
                logger.LogError($"{errors.Count()} {directoryName} issues will be located in {baseDirectory}");

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

        private static void CreateDirectoryAndMoveFile(ILogger logger, MoveFileOperation moveFileOperation, string baseDirectory, ErrorBase error)
        {
            string filenameWithExtension = Path.GetFileNameWithoutExtension(error.File);
            var directoryInfo = Path.Combine(baseDirectory, filenameWithExtension);
            var file = Path.Combine(baseDirectory, filenameWithExtension);

            // 1: make sure directory exists
            CreateDirectory(logger, directoryInfo);

            // 2: move the already copied file to the other duplicates s.t. we can investigate easily
            Move(logger, error, directoryInfo, file, moveFileOperation);
        }

        private static void HandleCollisionOrDuplicate(ILogger logger,
                                                       IFoundStatistics statistics,
                                                       CopyFileOperation copyFileOperation,
                                                       MoveFileOperation moveFileOperation,
                                                       DeleteFileOperation deleteFileOperation,
                                                       string baseDirectory,
                                                       FileAlreadyExistsError error)
        {
            if (IsDuplicate(logger, error, statistics))
            {
                logger.LogDebug($"removing duplicate {error.OtherFile} of {error.File}");
                deleteFileOperation.Delete(error.OtherFile);
            }
            else
            {
                HandleCollision(logger, baseDirectory, error, copyFileOperation, moveFileOperation);
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
                                            string errorBaseDirectory,
                                            FileAlreadyExistsError error,
                                            CopyFileOperation copyFileOperation,
                                            MoveFileOperation moveFileOperation)
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
            //  * move error1.FileInfo ("20230101/1.jpg") to ErrorFiles/20230101/1.jpg
            //  * copy error1.OtherFile with appended number to ErrorFiles/20230101/1_1.jpg
            // - process 2nd FileAlreadyExistsError: 
            //  * skip creating collision directory ErrorFiles/20230101
            //  * skip moving error2.FileInfo ("20230101/1.jpg") to ErrorFiles/20230101/1.jpg
            //  * copy error2.OtherFile with appended number to ErrorFiles/20230101/1_2.jpg

            string filenameWithExtension = Path.GetFileName(error.File);
            var (filename, extension) = SplitFileNameAndExtension(filenameWithExtension);
            var directoryInfo = Path.Combine(errorBaseDirectory, filename);
            var file = Path.Combine(errorBaseDirectory, filenameWithExtension);

            // 1: make sure directory exists
            CreateDirectory(logger, directoryInfo);

            // 2: move the already copied file to the other duplicates s.t. we can investigate easily
            Move(logger, error, directoryInfo, file, moveFileOperation);

            // 3: copy the other file into subdirectory with appended _number
            CopyFileWithAppendedNumber(logger, error.OtherFile, file, filename, extension, copyFileOperation);
        }

        private static void Move(ILogger logger,
                                 ErrorBase error,
                                 string collisionDirectory,
                                 string collisionFile,
                                 MoveFileOperation moveFileOperation)
        {
            var targetPath = Path.Combine(collisionDirectory, Path.GetFileName(collisionFile));

            if (File.Exists(error.File))
            {
                logger.LogTrace($"{error.File}: {moveFileOperation.ToString()} to where we store similar ones {targetPath}");
                moveFileOperation.ChangeFile(error.File, targetPath);
            }
            else
            {
                logger.LogError($"Cannot move {error.File}, as it does not exist.");
            }
        }

        private static void CreateDirectory(ILogger logger, string directory)
        {
            if (!Directory.Exists(directory))
            {
                logger.LogTrace("Creating {newDirectory}", directory);
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
            logger.LogDebug("Collision for {errorFileInfo}. Copy next to others as {fullname}", errorFile, fullname);
            copyFileOperation.ChangeFile(errorFile, fullname);
        }
    }
}