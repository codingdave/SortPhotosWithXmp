using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Operation;

public class DeleteFileOperation : IOperation
{
    private readonly ILogger _logger;
    private readonly IFile _fileWrapper;
    private readonly IDirectory _directoryWrapper;
    public bool IsForce { get; }
    public DirectoryStatistics DirectoryStatistics { get; }
    internal DeleteFileOperation(ILogger logger, IFile fileWrapper, IDirectory directoryWrapper, bool isForce)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileWrapper = fileWrapper ?? throw new ArgumentNullException(nameof(fileWrapper));
        _directoryWrapper = directoryWrapper ?? throw new ArgumentNullException(nameof(directoryWrapper));
        IsForce = isForce;
        DirectoryStatistics = new DirectoryStatistics();
    }

    public void DeleteFile(string file)
    {
        if (IsForce)
        {
            _logger.LogTrace($"IFile.Delete '{file}';");
            _fileWrapper.Delete(file);
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
            _directoryWrapper.Delete(directory, false);
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
            foreach (var subDirectory in _directoryWrapper.GetDirectories(path))
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
            if (_directoryWrapper.Exists(path))
            {
                DirectoryStatistics.DirectoriesFound.Add(path);
                if (
                    // if no directories are within this path
                    !_directoryWrapper.GetDirectories(path).Any()
                    // if no files are within this path
                    && !_directoryWrapper.GetFiles(path).Any()
                    // if path is not a file
                    && !_fileWrapper.Exists(path))
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