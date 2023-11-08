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
    public bool Force { get; }
    public DirectoriesDeletedResult Result { get; }
    public DirectoryStatistics DirectoryStatistics { get; }

    internal DeleteFileOperation(ILogger logger, IFile file, IDirectory directory, bool force)
    {
        _logger = logger;
        _file = file;
        _directory = directory;
        Force = force;
        Result = new DirectoriesDeletedResult(logger, directory, _directory.GetCurrentDirectory());
        DirectoryStatistics = new DirectoryStatistics();
    }

    public void Delete(string path)
    {
        if (Force)
        {
            _logger.LogTrace($"IFile.Delete '{path}';");
            _file.Delete(path);
        }
        else
        {
            _logger.LogTrace($"Ignoring IFile.Delete '{path}';");
        }
    }

    public void RecursivelyDeleteEmptyDirectories(string? path/*, bool isFirstRun = true*/)
    {
        if (path != null)
        {
            foreach (var subDirectory in _directory.GetDirectories(path))
            {
                RecursivelyDeleteEmptyDirectories(subDirectory/*, false*/);
                // DeleteDirectoryIfEmpty(subDirectory);
            }

            // if (isFirstRun)
            // {
            DeleteDirectoryIfEmpty(path);
            // }
        }

        void DeleteDirectoryIfEmpty(string path)
        {
            try
            {
                if (_directory.Exists(path))
                {
                    DirectoryStatistics.DirectoriesFound.Add(path);
                    // if no directories and no files are within this path
                    if (!_directory.GetDirectories(path).Any() && !_directory.GetFiles(path).Any())
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

    public void DeleteDirectory(string path)
    {
        if (Force)
        {
            _logger.LogTrace("IDirectory.Delete({path});", path);
            _directory.Delete(path, false);
        }
        else
        {
            _logger.LogTrace("Ignoring IDirectory.Delete({path});", path);
        }

        // when we simulate, we still want to count
        DirectoryStatistics.DirectoriesDeleted.Add(path);
    }
}