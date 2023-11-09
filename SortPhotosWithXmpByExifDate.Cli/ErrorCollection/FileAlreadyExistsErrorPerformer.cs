using System.Diagnostics;

using ImageMagick;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;


namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public class FileAlreadyExistsErrorPerformer : ErrorPerformerBase<FileAlreadyExistsError>
{
    public FileAlreadyExistsErrorPerformer(
        IErrorCollection<FileAlreadyExistsError> errorCollection,
        IFilesStatistics foundStatistics,
        IFile file,
        IDirectory directory,
        string baseDir,
        bool isForce) : base(errorCollection, foundStatistics, file, directory, baseDir, isForce)
    {
    }

    public override void Perform(ILogger logger)
    {
        if (_errorCollection.Errors.Any())
        {
            logger.LogInformation("Performing FileAlreadyExistsErrors");
            CollectCollisions(logger, _errorCollection.Errors,
                (FileDecomposition targetFile, FileAlreadyExistsError error)
                => HandleCollisionOrDuplicate(logger, error, targetFile));
        }
    }


    protected void HandleCollisionOrDuplicate(ILogger logger, FileAlreadyExistsError error, FileDecomposition targetFile)
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

    private bool IsDuplicate(ILogger logger, FileAlreadyExistsError error)
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
        #warning use the FileScanner
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

}