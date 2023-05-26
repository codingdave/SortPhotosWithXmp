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
            var copyFileOperation = new CopyFileOperation(logger, statistics.FileOperation.IsChanging);
            var moveFileOperation = new MoveFileOperation(logger, statistics.FileOperation.IsChanging);
            CollectCollisions(logger, statistics, errorCollection.Errors.OfType<FileAlreadyExistsError>(), copyFileOperation, moveFileOperation);
            CollectErrors(logger, errorCollection.Errors.OfType<NoTimeFoundError>(), moveFileOperation);
            CollectErrors(logger, errorCollection.Errors.OfType<MetaDataError>(), moveFileOperation);
        }

        private static void CollectErrors<T>(ILogger logger,
                                             IEnumerable<T> errors,
                                             MoveFileOperation moveFileOperation) where T: ErrorBase
        {
            if (errors.Any())
            {
                var directoryName = nameof(T);
                MoveIssuesToCommonDirectory(logger, errors, moveFileOperation, directoryName);
            }
        }

        private static void MoveIssuesToCommonDirectory(ILogger logger, IEnumerable<ErrorBase> errors, MoveFileOperation moveFileOperation, string directoryName)
        {

            var baseDirectory = new DirectoryInfo(directoryName);
            logger.LogError($"{errors.Count()} {directoryName} issues will be located in {baseDirectory.FullName}");

            RenamePossiblyExistingDirectory(logger, baseDirectory);
            CreateDirectory(logger, baseDirectory);

            foreach (var error in errors)
            {
                try
                {
                    string filenameWithExtension = error.FileInfo.Name;
                    var (filename, extension) = SplitFileNameAndExtension(filenameWithExtension);
                    var directoryInfo = new DirectoryInfo(Path.Combine(baseDirectory.FullName, filename));
                    var fileInfo = new FileInfo(Path.Combine(baseDirectory.FullName, filenameWithExtension));

                    // 1: make sure directory exists
                    CreateDirectory(logger, directoryInfo, fileInfo);

                    // 2: move the already copied file to the other duplicates s.t. we can investigate easily
                    Move(logger, error, directoryInfo, fileInfo, moveFileOperation);
                }
                catch (Exception e)
                {
                    logger.LogExceptionError(e);
                }
            }
        }

        private static void CollectCollisions(ILogger logger,
                                              IFoundStatistics statistics,
                                              IEnumerable<FileAlreadyExistsError> errors,
                                              CopyFileOperation copyFileOperation,
                                              MoveFileOperation moveFileOperation)
        {
            var deleteFileOperation = new DeleteFileOperation(logger, copyFileOperation.IsChanging);
            if (errors.Any())
            {
                var errorBaseDirectory = new DirectoryInfo(nameof(FileAlreadyExistsError));
                logger.LogError($"Could not copy all files due to collisions. Please check {errorBaseDirectory.FullName}");

                RenamePossiblyExistingDirectory(logger, errorBaseDirectory);
                CreateDirectory(logger, errorBaseDirectory);

                logger.LogTrace($"Copy {errors.Count()} files to {errorBaseDirectory.FullName}");
                foreach (var error in errors)
                {
                    try
                    {
                        if (IsDuplicate(logger, error, statistics))
                        {
                            logger.LogDebug($"removing duplicate {error.OtherFile} of {error.FileInfo}");
                            deleteFileOperation.Delete(error.OtherFile.FullName);
                        }
                        else
                        {
                            HandleCollision(logger, errorBaseDirectory, error, copyFileOperation, moveFileOperation);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogExceptionError(e);
                    }
                }
            }
        }

        private static bool IsDuplicate(ILogger logger,
                                        FileAlreadyExistsError error,
                                        IFoundStatistics statistics)
        {
            // when are 2 images identical?
            var isDuplicate = false;

            // same type (image vs xmp)
            var extensionFile = error.FileInfo.Extension;
            var extensionOther = error.OtherFile.Extension;
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
            using var fileStream = File.OpenRead(error.FileInfo.FullName);
            using var otherfileStream = File.OpenRead(error.OtherFile.FullName);
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
                using var copiedImage = new MagickImage(error.FileInfo);
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
                                            DirectoryInfo errorBaseDirectory,
                                            FileAlreadyExistsError error,
                                            CopyFileOperation copyFileOperation,
                                            MoveFileOperation moveFileOperation)
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
            var directoryInfo = new DirectoryInfo(Path.Combine(errorBaseDirectory.FullName, filename));
            var fileInfo = new FileInfo(Path.Combine(errorBaseDirectory.FullName, filenameWithExtension));

            // 1: make sure directory exists
            CreateDirectory(logger, directoryInfo, fileInfo);

            // 2: move the already copied file to the other duplicates s.t. we can investigate easily
            Move(logger, error, directoryInfo, fileInfo, moveFileOperation);

            // 3: copy the other file into subdirectory with appended _number
            CopyFileWithAppendedNumber(logger, error.OtherFile, fileInfo, filename, extension, copyFileOperation);
        }

        private static void Move(ILogger logger,
                                 ErrorBase error,
                                 DirectoryInfo collisionDirectoryInfo,
                                 FileInfo collisionFileInfo,
                                 MoveFileOperation moveFileOperation)
        {
            var targetPath = Path.Combine(collisionDirectoryInfo.FullName, collisionFileInfo.Name);

            if (error.FileInfo.Exists)
            {
                logger.LogTrace($"{error.FileInfo.FullName}: {moveFileOperation.ToString()} to where we store similar ones {targetPath}");
                moveFileOperation.ChangeFile(error.FileInfo.FullName, targetPath);
            }
            else
            {
                logger.LogError($"Cannot move {error.FileInfo.FullName}, as it does not exist.");
            }
        }

        private static void CreateDirectory(ILogger logger, DirectoryInfo directoryInfo, FileInfo fileInfo)
        {
            if (!directoryInfo.Exists)
            {
                logger.LogTrace("Creating {newDirectory}", fileInfo.FullName);
                directoryInfo.Create();
            }
        }

        private static void CreateDirectory(ILogger logger, DirectoryInfo errorBaseDirectory)
        {
            if (!Directory.Exists(errorBaseDirectory.FullName))
            {
                logger.LogTrace("Creating {newDirector}", errorBaseDirectory.FullName);
                Directory.CreateDirectory(errorBaseDirectory.FullName);
            }
        }

        private static void RenamePossiblyExistingDirectory(ILogger logger, DirectoryInfo directory)
        {
            // rename possibly existing ErrorFiles directory (add lastWriteTime to the end)
            if (directory.Exists)
            {
                var time = File.GetLastWriteTime(directory.FullName).ToString("yyyyMMddTHHmmss");
                var parentDirectory = directory.Parent ?? throw new InvalidOperationException("Path does not exist");
                var directoryName = directory.Name;
                var newName = Path.Combine(parentDirectory.FullName, directoryName + "_" + time);
                logger.LogTrace("Renaming {oldDirectory} to {newDirectory}", directory.FullName, newName);
                Directory.Move(directory.FullName, newName);
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
                                                       FileInfo errorFileInfo,
                                                       FileInfo collisionFileInfo,
                                                       string filename,
                                                       string extension,
                                                       CopyFileOperation copyFileOperation)
        {
            // copy this file into subdirectory with appended _number
            var directory = collisionFileInfo.Directory ?? throw new InvalidOperationException("Directory does not exist");
            var subdirectory = Path.Combine(directory.FullName, filename);
            var fileCount = Directory.GetFiles(subdirectory, "*" + extension).Length;
            var fullname = Path.Combine(subdirectory, filename + "_" + fileCount + extension);
            logger.LogDebug("Collision for {errorFileInfo}. Copy next to others as {fullname}", errorFileInfo, fullname);
            copyFileOperation.ChangeFile(errorFileInfo.FullName, fullname);
        }
    }
}