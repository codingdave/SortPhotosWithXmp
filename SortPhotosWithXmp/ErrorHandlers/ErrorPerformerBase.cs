using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Operation;
using SortPhotosWithXmp.Repository;
using SortPhotosWithXmp.Statistics;

using SystemInterface.IO;


namespace SortPhotosWithXmp.Performer;

public abstract class ErrorPerformerBase<T> : IPerformer where T : IError
{
    protected readonly IErrorCollection<T> _errorCollection = new ErrorCollection<T>();
    protected readonly IFilesStatistics _filesStatistics;
    protected readonly IFile _file;
    protected readonly IDirectory _directory;
    private readonly string _baseDir;
    protected readonly bool _isForce;

    public ErrorPerformerBase(
        IFilesStatistics filesStatistics,
        IFile file,
        IDirectory directory,
        string baseDir,
        bool isForce)
    {
        _filesStatistics = filesStatistics;
        _file = file;
        _directory = directory;
        _baseDir = baseDir;
        _isForce = isForce;
    }

    public IErrorCollection<T> Errors => _errorCollection;

    public abstract void Perform(ILogger logger);

    protected void CollectCollisions(ILogger logger, IEnumerable<T> errors, Action<FileDecomposition, T> action, Operations operations)
    {
        if (errors.Any())
        {
            var directoryName = errors.First().Name;
            var targetDirectory = operations.MoveFileOperation.JoinDirectory(_baseDir, directoryName);

            logger.LogError($"{errors.Count()} {directoryName} issues will be located in the directory '{targetDirectory}'");

            RenamePossiblyExistingDirectory(logger, targetDirectory, operations.MoveFileOperation);
            operations.CopyFileOperation.CreateDirectory(targetDirectory);

            foreach (var error in errors)
            {
                try
                {
                    var file = CreateFileDecompositionAtCollisionDirectory(targetDirectory, error.File, operations.MoveFileOperation);
                    action(file, error);
                }
                catch (Exception e)
                {
                    logger.LogExceptionError(e);
                }
            }
        }
    }

    protected void CreateDirectoryAndCopyFile(ILogger logger, ErrorBase error, FileDecomposition targetFile, MoveFileOperation moveFileOperation, CopyFileOperation copyFileOperation)
    {
        // 1: make sure directory exists
        copyFileOperation.CreateDirectory(targetFile.Directory);

        // 2: copy the first file of the collision to the other duplicates s.t. we can investigate easily
        CopyFileWithAppendedNumber(logger, error.File, targetFile, moveFileOperation, copyFileOperation);
    }

    private void RenamePossiblyExistingDirectory(ILogger logger, string sourceDirName, MoveFileOperation moveFileOperation)
    {
        // rename possibly existing ErrorFiles directory (add lastWriteTime to the end)
        if (_directory.Exists(sourceDirName))
        {
            var time = File.GetLastWriteTime(sourceDirName).ToString("yyyyMMddTHHmmss");
            var d = new DirectoryInfo(sourceDirName);
            var parentDirectory = d.Parent ?? throw new InvalidOperationException("Parent of path does not exist");
            var directoryName = d.Name;
            var destDirName = moveFileOperation.JoinDirectory(parentDirectory.FullName, directoryName + "_" + time);
            logger.LogTrace("Renaming '{oldDirectory}' to '{newDirectory}'", sourceDirName, destDirName);
            moveFileOperation.RenameDirectory(sourceDirName, destDirName);
        }
    }

    private FileDecomposition CreateFileDecompositionAtCollisionDirectory(string targetDirectory, string errorFile, MoveFileOperation moveFileOperation)
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
        var directory = moveFileOperation.JoinDirectory(targetDirectory, filename);
        var completeFilepath = moveFileOperation.JoinFile(directory, filenameWithExtension);

        var targetFile = new FileDecomposition(completeFilepath, directory, filename, extension);
        return targetFile;
    }

    protected void CopyFileWithAppendedNumber(ILogger logger, string errorFile, FileDecomposition targetFile, MoveFileOperation moveFileOperation, CopyFileOperation copyFileOperation)
    {
        // copy this file into subdirectory with appended _number
        var path = targetFile.Directory;
        var fileCount = _directory.GetFiles(path, "*" + targetFile.Extension).Length;
        var numberString = fileCount > 0 ? "_" + fileCount : string.Empty;
        var filenameWithExtension = targetFile.Name + numberString + targetFile.Extension;
        var fullname = moveFileOperation.JoinFile(path, filenameWithExtension);
        logger.LogTrace("Collision for '{errorFile}'. Arrange next to others as '{fullname}'", errorFile, fullname);
        copyFileOperation.ChangeFiles(new List<IImageFile>() { new ImageFile(errorFile) }, fullname);
    }
}
