using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;


namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

public abstract class ErrorPerformerBase<T> : IPerformer where T : IError
{
    protected readonly IErrorCollection<T> _errorCollection;
    protected readonly IFilesStatistics _foundStatistics;
    protected readonly IFile _file;
    protected readonly IDirectory _directory;
    private readonly string _baseDir;
    protected readonly bool _isForce;
    protected CopyFileOperation _copyFileOperation;
    protected MoveFileOperation _moveFileOperation;
    protected DeleteFileOperation _deleteFileOperation;

    public ErrorPerformerBase(
        IErrorCollection<T> errorCollection,
        IFilesStatistics foundStatistics,
        IFile file,
        IDirectory directory,
        string baseDir,
        bool isForce)
    {
        _errorCollection = errorCollection;
        _foundStatistics = foundStatistics;
        _file = file;
        _directory = directory;
        _baseDir = baseDir;
        _isForce = isForce;
    }

    public abstract void Perform(ILogger logger);

    protected void CollectCollisions(ILogger logger, IEnumerable<T> errors, Action<FileDecomposition, T> action)
    {
        if (errors.Any())
        {
            // when we have an error, we want to copy
            var isCopyingEnforced = true;
#warning copy and move should HAVE not are a directory operator
            _copyFileOperation = new CopyFileOperation(logger, _file, _directory, isCopyingEnforced);
            _moveFileOperation = new MoveFileOperation(logger, _file, _directory, isCopyingEnforced);
            _deleteFileOperation = new DeleteFileOperation(logger, _file, _directory, _isForce);

            var directoryName = errors.First().Name;
            var targetDirectory = _moveFileOperation.JoinDirectory(_baseDir, directoryName);

            logger.LogError($"{errors.Count()} {directoryName} issues will be located in the directory '{targetDirectory}'");

            RenamePossiblyExistingDirectory(logger, targetDirectory);
            _copyFileOperation.CreateDirectory(targetDirectory);

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

    protected void CreateDirectoryAndCopyFile(ILogger logger, ErrorBase error, FileDecomposition targetFile)
    {
        // 1: make sure directory exists
        _copyFileOperation.CreateDirectory(targetFile.Directory);

        // 2: copy the first file of the collision to the other duplicates s.t. we can investigate easily
        CopyFileWithAppendedNumber(logger, error.File, targetFile);
    }

    private void RenamePossiblyExistingDirectory(ILogger logger, string sourceDirName)
    {
        // rename possibly existing ErrorFiles directory (add lastWriteTime to the end)
        if (_directory.Exists(sourceDirName))
        {
            var time = File.GetLastWriteTime(sourceDirName).ToString("yyyyMMddTHHmmss");
            var d = new DirectoryInfo(sourceDirName);
            var parentDirectory = d.Parent ?? throw new InvalidOperationException("Parent of path does not exist");
            var directoryName = d.Name;
            var destDirName = _moveFileOperation.JoinDirectory(parentDirectory.FullName, directoryName + "_" + time);
            logger.LogTrace("Renaming '{oldDirectory}' to '{newDirectory}'", sourceDirName, destDirName);
            _moveFileOperation.RenameDirectory(sourceDirName, destDirName);
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
        var directory = _moveFileOperation.JoinDirectory(targetDirectory, filename);
        var completeFilepath = _moveFileOperation.JoinFile(directory, filenameWithExtension);

        var targetFile = new FileDecomposition(completeFilepath, directory, filename, extension);
        return targetFile;
    }

    protected void CopyFileWithAppendedNumber(ILogger logger, string errorFile, FileDecomposition targetFile)
    {
        // copy this file into subdirectory with appended _number
        var path = targetFile.Directory;
        var fileCount = _directory.GetFiles(path, "*" + targetFile.Extension).Length;
        var numberString = fileCount > 0 ? "_" + fileCount : string.Empty;
        var filenameWithExtension = targetFile.Name + numberString + targetFile.Extension;
        var fullname = _moveFileOperation.JoinFile(path, filenameWithExtension);
        logger.LogTrace("Collision for '{errorFile}'. Arrange next to others as '{fullname}'", errorFile, fullname);
        _copyFileOperation.ChangeFiles(new List<IImageFile>() { new ImageFile(errorFile) }, fullname);
    }
}
