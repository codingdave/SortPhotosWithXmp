using System.Diagnostics;

using ImageMagick;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class ErrorCollectionPerformer : IPerformer
{
    private readonly IReadOnlyErrorCollection _errorCollection;
    private readonly IFilesStatistics _foundStatistics;
    private readonly IFile _file;
    private readonly IDirectory _directory;
    private readonly string _baseDir;
    private readonly bool _isForce;
    private CopyFileOperation _copyFileOperation;
    private DeleteFileOperation _deleteFileOperation;

    public ErrorCollectionPerformer(IReadOnlyErrorCollection errorCollection, IFilesStatistics foundStatistics, IFile file, IDirectory directory, string baseDir, bool isForce)
    {
        _errorCollection = errorCollection;
        _foundStatistics = foundStatistics;
        _file = file;
        _directory = directory;
        _baseDir = baseDir;
        _isForce = isForce;
    }

    public void Perform(ILogger logger)
    {
        // when we have an error, we want to copy
        var isCopyingEnforced = true;
        _copyFileOperation = new CopyFileOperation(logger, _file, _directory, isCopyingEnforced);
        _deleteFileOperation = new DeleteFileOperation(logger, _file, _directory, _isForce);

        CollectCollisions(logger, _errorCollection.Errors.OfType<FileAlreadyExistsError>(),
            (FileDecomposition targetFile, FileAlreadyExistsError error)
            => HandleCollisionOrDuplicate(logger, error, targetFile));
        CollectCollisions(logger, _errorCollection.Errors.OfType<NoTimeFoundError>(),
            (FileDecomposition targetFile, NoTimeFoundError error)
            => CreateDirectoryAndCopyFile(logger, error, targetFile));
        CollectCollisions(logger, _errorCollection.Errors.OfType<MetaDataError>(),
            (FileDecomposition targetFile, MetaDataError error)
            => CreateDirectoryAndCopyFile(logger, error, targetFile));
    }

    private void CollectCollisions<T>(
        ILogger logger,
        IEnumerable<T> errors,
        Action<FileDecomposition, T> action) where T : ErrorBase
    {
        if (errors.Any())
        {
            var directoryName = errors.First().Name;
            var targetDirectory = Path.Join(_baseDir, directoryName);

            logger.LogError($"{errors.Count()} {directoryName} issues will be located in the directory '{targetDirectory}'");

            RenamePossiblyExistingDirectory(logger, targetDirectory);
            CreateDirectory(logger, targetDirectory);

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

    private void CreateDirectoryAndCopyFile(ILogger logger, ErrorBase error, FileDecomposition targetFile)
    {
        // 1: make sure directory exists
        CreateDirectory(logger, targetFile.Directory);

        // 2: copy the first file of the collision to the other duplicates s.t. we can investigate easily
        CopyFileWithAppendedNumber(logger, error.File, targetFile);
    }

    private void HandleCollisionOrDuplicate(
        ILogger logger,
        FileAlreadyExistsError error,
        FileDecomposition targetFile)
    {
        if (IsDuplicate(logger, error))
        {
            HandleDuplicate(logger, error);
        }
        else
        {
            HandleCollision(logger, targetFile, error);
        }
    }

    private void HandleDuplicate(ILogger logger, FileAlreadyExistsError error)
    {
        logger.LogDebug($"{error.OtherFile} is duplicate of {error.File}");
        _deleteFileOperation.DeleteFile(error.OtherFile);
    }

    private bool IsDuplicate(
        ILogger logger,
        FileAlreadyExistsError error)
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
                ? AreXmpsDuplicates(error)
                : AreImagesDuplicates(logger, error);
        }

        return isDuplicate;
    }

    private bool AreXmpsDuplicates(FileAlreadyExistsError error)
    {
        // xmps are identical, if their hash is identical

        using var md5 = System.Security.Cryptography.MD5.Create();
        using var fileStream = _file.OpenRead(error.File);
        using var otherfileStream = _file.OpenRead(error.OtherFile);
        var hash1 = md5.ComputeHash(fileStream.FileStreamInstance);
        var hash2 = md5.ComputeHash(otherfileStream.FileStreamInstance);
        var isHashIdentical = hash1 == hash2;
        if (isHashIdentical)
        {
            _foundStatistics.SkippedXmps++;
        }

        return isHashIdentical;
    }

    private bool AreImagesDuplicates(ILogger logger, FileAlreadyExistsError error)
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
                _foundStatistics.SkippedImages++;
            }
        }
        catch (Exception e)
        {
            logger.LogExceptionError(e);
        }

        return isDuplicate;
    }

    private void HandleCollision(
        ILogger logger,
        FileDecomposition targetFile,
        FileAlreadyExistsError error)
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

        CreateDirectoryAndCopyFile(logger, error, targetFile);

        // 3: copy the other file into subdirectory with appended _number
        CopyFileWithAppendedNumber(logger, error.OtherFile, targetFile);
    }

    private void CreateDirectory(ILogger logger, string path)
    {
        #warning Creaing directory 03-10-24_Opa_Lan_bis_03-10-26_032.JPG
        if (!_directory.Exists(path))
        {
            logger.LogTrace("Creating directory '{newDirectory}'", path);
            _ = _directory.CreateDirectory(path);
        }
    }

    private void RenamePossiblyExistingDirectory(ILogger logger, string path)
    {
        // rename possibly existing ErrorFiles directory (add lastWriteTime to the end)
        if (_directory.Exists(path))
        {
            var time = File.GetLastWriteTime(path).ToString("yyyyMMddTHHmmss");
            var d = new DirectoryInfo(path);
            var parentDirectory = d.Parent ?? throw new InvalidOperationException("Path does not exist");
            var directoryName = d.Name;
            var newName = Path.Combine(parentDirectory.FullName, directoryName + "_" + time);
            logger.LogTrace("Renaming '{oldDirectory}' to '{newDirectory}'", path, newName);
            _directory.Move(path, newName);
        }
    }

    private FileDecomposition CreateFileDecompositionAtCollisionDirectory(string targetDirectory, string errorFile)
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

    private void CopyFileWithAppendedNumber(
        ILogger logger,
        string errorFile,
        FileDecomposition targetFile)
    {
        // copy this file into subdirectory with appended _number
        var path = targetFile.Directory;
        var fileCount = _directory.GetFiles(path, "*" + targetFile.Extension).Length;
        var numberString = fileCount > 0 ? "_" + fileCount : string.Empty;
        var fullname = Path.Combine(path, targetFile.Name + numberString + targetFile.Extension);
        logger.LogDebug("Collision for '{errorFile}'. Arrange next to others as '{fullname}'", errorFile, fullname);
        _copyFileOperation.ChangeFiles(new List<IImageFile>() { new ImageFile("errorFile") }, fullname);
    }
}
