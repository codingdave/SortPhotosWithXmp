using Microsoft.Extensions.Logging;
using ImageMagick;
using System.Diagnostics;
using SortPhotosWithXmpByExifDate.Cli.Result;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli
{
    public static class IReadOnlyListExtensions
    {
        public static void HandleErrorFiles(this IReadOnlyErrorCollection errorCollection,
            ILogger logger,
            IFoundStatistics foundStatistics,
            IFile file,
            IDirectory directory,
            bool force)
        {
            var copyFileOperation = new CopyFileOperation(logger, file, directory, force);
            var deleteFileOperation = new DeleteFileOperation(logger, file, directory, force);

            CollectCollisions(logger, directory, errorCollection.Errors.OfType<FileAlreadyExistsError>(), 
                (FileDecomposition targetFile, FileAlreadyExistsError error) 
                => HandleCollisionOrDuplicate(logger,file, directory, foundStatistics, copyFileOperation, deleteFileOperation, error, targetFile));
            CollectCollisions(logger, directory, errorCollection.Errors.OfType<NoTimeFoundError>(), 
                (FileDecomposition targetFile, NoTimeFoundError error) 
                => CreateDirectoryAndCopyFile(logger, directory, error, targetFile, copyFileOperation));
            CollectCollisions(logger, directory, errorCollection.Errors.OfType<MetaDataError>(), 
                (FileDecomposition targetFile, MetaDataError error) 
                => CreateDirectoryAndCopyFile(logger, directory, error, targetFile, copyFileOperation));
        }

        private static void CollectCollisions<T>(ILogger logger,
                                              IDirectory directory,
                                              IEnumerable<T> errors,
                                              Action<FileDecomposition, T> action) where T : ErrorBase
        {
            if (errors.Any())
            {
                var directoryName = errors.First().Name;
                var targetDirectory = new DirectoryInfo(directoryName).FullName;

                logger.LogError($"{errors.Count()} {directoryName} issues will be located in the directory '{targetDirectory}'");

                RenamePossiblyExistingDirectory(logger, directory, targetDirectory);
                CreateDirectory(logger, directory, targetDirectory);

                foreach (var error in errors)
                {
                    try
                    {
                        var file = CreateFileDecompositionAtCollisionDirectory(targetDirectory, error.File);
                        action(file, error);
                    }
                    catch (Exception e)
                    {
                        logger.LogExceptionError(e);
                    }
                }
            }
        }

        private static void CreateDirectoryAndCopyFile(ILogger logger, IDirectory directory, ErrorBase error, FileDecomposition targetFile, CopyFileOperation copyFileOperation)
        {
            // 1: make sure directory exists
            CreateDirectory(logger, directory, targetFile.Directory);

            // 2: copy the first file of the collision to the other duplicates s.t. we can investigate easily
            CopyFileWithAppendedNumber(logger, directory, error.File, targetFile, copyFileOperation);
        }

        private static void HandleCollisionOrDuplicate(ILogger logger,
                                                       IFile file,
                                                       IDirectory directory,
                                                       IFoundStatistics foundStatistics,
                                                       CopyFileOperation copyFileOperation,
                                                       DeleteFileOperation deleteFileOperation,
                                                       FileAlreadyExistsError error,
                                                       FileDecomposition targetFile)
        {
            if (IsDuplicate(logger, error, foundStatistics, file))
            {
                HandleDuplicate(logger, deleteFileOperation, error);
            }
            else
            {
                HandleCollision(logger, directory, targetFile, error, copyFileOperation);
            }
        }

        private static void HandleDuplicate(ILogger logger, DeleteFileOperation deleteFileOperation, FileAlreadyExistsError error)
        {
            logger.LogDebug($"{error.OtherFile} is duplicate of {error.File}");
            deleteFileOperation.Delete(error.OtherFile);
        }

        private static bool IsDuplicate(ILogger logger,
                                        FileAlreadyExistsError error,
                                        IFoundStatistics foundStatistics,
                                        IFile file)
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
                isDuplicate = extensionFile.EndsWith(FileScanner.XmpExtension, StringComparison.OrdinalIgnoreCase)
                    ? AreXmpsDuplicates(error, foundStatistics, file)
                    : AreImagesDuplicates(logger, error, foundStatistics);
            }

            return isDuplicate;
        }

        private static bool AreXmpsDuplicates(FileAlreadyExistsError error,
                                              IFoundStatistics foundStatistics,
                                              IFile file)
        {
            // xmps are identical, if their hash is identical

            using var md5 = System.Security.Cryptography.MD5.Create();
            using var fileStream = file.OpenRead(error.File);
            using var otherfileStream = file.OpenRead(error.OtherFile);
            var hash1 = md5.ComputeHash(fileStream.FileStreamInstance);
            var hash2 = md5.ComputeHash(otherfileStream.FileStreamInstance);
            var isHashIdentical = hash1 == hash2;
            if (isHashIdentical)
            {
                foundStatistics.SkippedXmps++;
            }

            return isHashIdentical;
        }

        private static bool AreImagesDuplicates(ILogger logger,
                                                FileAlreadyExistsError error,
                                                IFoundStatistics foundStatistics)
        {
            var isDuplicate = false;

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
                    foundStatistics.SkippedImages++;
                }
            }
            catch (Exception e)
            {
                logger.LogExceptionError(e);
            }

            return isDuplicate;
        }

        private static void HandleCollision(ILogger logger,
                                            IDirectory directory,
                                            FileDecomposition targetFile,
                                            FileAlreadyExistsError error,
                                            CopyFileOperation copyFileOperation)
        {
            logger.LogDebug($"Handling collision between {error.File} and {error.OtherFile}!");

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
            //  * copy error1.File ("20230101/1.jpg") to ErrorFiles/20230101/1.jpg
            //  * copy error1.OtherFile with appended number to ErrorFiles/20230101/1_1.jpg
            // - process 2nd FileAlreadyExistsError: 
            //  * skip creating collision directory ErrorFiles/20230101
            //  * skip copying error2.File ("20230101/1.jpg") to ErrorFiles/20230101/1.jpg
            //  * copy error2.OtherFile with appended number to ErrorFiles/20230101/1_2.jpg

            CreateDirectoryAndCopyFile(logger, directory, error, targetFile, copyFileOperation);

            // 3: copy the other file into subdirectory with appended _number
            CopyFileWithAppendedNumber(logger, directory, error.OtherFile, targetFile, copyFileOperation);
        }

        private static void CreateDirectory(ILogger logger, IDirectory directory, string path)
        {
            if (!directory.Exists(path))
            {
                logger.LogDebug("Creating directory '{newDirectory}'", path);
                _ = directory.CreateDirectory(path);
            }
        }

        private static void RenamePossiblyExistingDirectory(ILogger logger, IDirectory directory, string path)
        {
            // rename possibly existing ErrorFiles directory (add lastWriteTime to the end)
            if (directory.Exists(path))
            {
                var time = File.GetLastWriteTime(path).ToString("yyyyMMddTHHmmss");
                var d = new DirectoryInfo(path);
                var parentDirectory = d.Parent ?? throw new InvalidOperationException("Path does not exist");
                var directoryName = d.Name;
                var newName = Path.Combine(parentDirectory.FullName, directoryName + "_" + time);
                logger.LogTrace("Renaming '{oldDirectory}' to '{newDirectory}'", path, newName);
                directory.Move(path, newName);
            }
        }

        private static FileDecomposition CreateFileDecompositionAtCollisionDirectory(string targetDirectory, string errorFile)
        {
            static (string filename, string extension) SplitFileNameAndExtension(string name)
            {
                // supports "img1234.jpg.xmp" => ("img1234", ".jpg.xmp")
                var ret = (string.Empty, string.Empty);
                var dotPosition = name.IndexOf('.');
                if (dotPosition > 0)
                {
                    ret = (name[..dotPosition], name[dotPosition..]);
                }
                return ret;
            }

            var filenameWithExtension = Path.GetFileName(errorFile);
            var (filename, extension) = SplitFileNameAndExtension(filenameWithExtension);
            var directory = Path.Combine(targetDirectory, filename);
            var completeFilepath = Path.Combine(directory, filenameWithExtension);

            var targetFile = new FileDecomposition(completeFilepath, directory, filename, extension);
            Serilog.Log.Verbose($"{nameof(targetDirectory)}: {targetDirectory}, errorFile: {errorFile} => {nameof(targetFile)}: {targetFile}");

            return targetFile;
        }

        private static void CopyFileWithAppendedNumber(ILogger logger,
                                                       IDirectory directory,
                                                       string errorFile,
                                                       FileDecomposition targetFile,
                                                       CopyFileOperation copyFileOperation)
        {
            // copy this file into subdirectory with appended _number
            var path = targetFile.Directory;
            var fileCount = directory.GetFiles(path, "*" + targetFile.Extension).Length;
            var numberString = fileCount > 0 ? "_" + fileCount : string.Empty;
            var fullname = Path.Combine(path, targetFile.Name + numberString + targetFile.Extension);
            logger.LogDebug("Collision for '{errorFile}'. Arrange next to others as '{fullname}'", errorFile, fullname);
            copyFileOperation.ChangeFiles(new List<IImageFile>() { new ImageFile("errorFile") }, fullname);
        }
    }
}