using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations;

public class DeleteFileOperation : IOperation
{
    private readonly ILogger _logger;
    private readonly IFile _file;
    private readonly IDirectory _directory;
    public bool IsForce { get; }
    public DirectoryStatistics DirectoryStatistics { get; }
    internal DeleteFileOperation(ILogger logger, IFile file, IDirectory directory, bool isForce)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _file = file ?? throw new ArgumentNullException(nameof(file));
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        IsForce = isForce;
        DirectoryStatistics = new DirectoryStatistics();
    }

    public void DeleteFile(string file)
    {
        if (IsForce)
        {
            _logger.LogTrace($"IFile.Delete '{file}';");
            _file.Delete(file);
        }
        else
        {
            _logger.LogTrace($"Ignoring IFile.Delete '{file}';");
        }
    }

    public void DeleteDirectory(string directory)
    {
        if (IsForce)
        {
            _logger.LogTrace("IDirectory.Delete({path});", directory);
            _directory.Delete(directory, false);
        }
        else
        {
            _logger.LogTrace("Ignoring IDirectory.Delete({path});", directory);
        }

        // when we simulate, we still want to count
        DirectoryStatistics.DirectoriesDeleted.Add(directory);
    }

    public void RecursivelyDeleteEmptyDirectories(string? path)
    {
        if (path != null)
        {
            foreach (var subDirectory in _directory.GetDirectories(path))
            {
                RecursivelyDeleteEmptyDirectories(subDirectory);
            }
            
            DeleteDirectoryIfEmpty(path);
        }
    }

    private void DeleteDirectoryIfEmpty(string path)
    {
        try
        {
            if (_directory.Exists(path))
            {
                DirectoryStatistics.DirectoriesFound.Add(path);
                if (
                    // if no directories are within this path
                    !_directory.GetDirectories(path).Any()
                    // if no files are within this path
                    && !_directory.GetFiles(path).Any()
                    // if path is not a file
                    && !_file.Exists(path))
                {
                    DeleteDirectory(path);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogExceptionError(e);
        }
    }
}