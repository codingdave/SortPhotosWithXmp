using System.Diagnostics;
using System.Security.AccessControl;

using Microsoft.Extensions.Logging;

using SystemInterface;

using SystemInterface.IO;
using SystemInterface.Security.AccessControl;

using SystemWrapper.IO;

namespace SortPhotosWithXmpByExifDate.Cli;

internal class DirectoryLogger : IDirectory
{
    private readonly ILogger<CommandLine> _logger;

    public DirectoryLogger(ILogger<CommandLine> logger) => _logger = logger;

    [DebuggerStepThrough]
    public IDirectoryInfo CreateDirectory(string path)
    {
        // TODO: Not yet implemented
        _ = Directory.CreateDirectory(path);
        // return new DirectoryInfoWrap(path);
        return null;
    }

    public IDirectoryInfo CreateDirectory(string path, IDirectorySecurity directorySecurity)
    {
        throw new NotImplementedException();
    }

    public void Delete(string path)
    {
        throw new NotImplementedException();
    }

    public void Delete(string path, bool recursive)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> EnumerateFiles(string path)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
        throw new NotImplementedException();
    }

    [DebuggerStepThrough]
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFiles(path, searchPattern, searchOption);
    }

    [DebuggerStepThrough]
    public bool Exists(string path)
    {
        return Directory.Exists(path);
    }

    public IDirectorySecurity GetAccessControl(string path)
    {
        throw new NotImplementedException();
    }

    public IDirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
    {
        throw new NotImplementedException();
    }

    public IDateTime GetCreationTime(string path)
    {
        throw new NotImplementedException();
    }

    public IDateTime GetCreationTimeUtc(string path)
    {
        throw new NotImplementedException();
    }

    [DebuggerStepThrough]
    public string GetCurrentDirectory()
    {
        return Directory.GetCurrentDirectory();
    }

    [DebuggerStepThrough]
    public string[] GetDirectories(string path)
    {
        return Directory.GetDirectories(path);
    }

    public string[] GetDirectories(string path, string searchPattern)
    {
        throw new NotImplementedException();
    }

    public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        throw new NotImplementedException();
    }

    public string GetDirectoryRoot(string path)
    {
        throw new NotImplementedException();
    }

    [DebuggerStepThrough]
    public string[] GetFiles(string path)
    {
        return Directory.GetFiles(path);
    }

    [DebuggerStepThrough]
    public string[] GetFiles(string path, string searchPattern)
    {
        return Directory.GetFiles(path, searchPattern);
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        throw new NotImplementedException();
    }

    public string[] GetFileSystemEntries(string path)
    {
        throw new NotImplementedException();
    }

    public string[] GetFileSystemEntries(string path, string searchPattern)
    {
        throw new NotImplementedException();
    }

    public IDateTime GetLastAccessTime(string path)
    {
        throw new NotImplementedException();
    }

    public IDateTime GetLastAccessTimeUtc(string path)
    {
        throw new NotImplementedException();
    }

    public IDateTime GetLastWriteTime(string path)
    {
        throw new NotImplementedException();
    }

    public IDateTime GetLastWriteTimeUtc(string path)
    {
        throw new NotImplementedException();
    }

    public string[] GetLogicalDrives()
    {
        throw new NotImplementedException();
    }

    public IDirectoryInfo GetParent(string path)
    {
        throw new NotImplementedException();
    }

    [DebuggerStepThrough]
    public void Move(string sourceDirName, string destDirName)
    {
        Directory.Move(sourceDirName, destDirName);
    }

    public void SetAccessControl(string path, IDirectorySecurity directorySecurity)
    {
        throw new NotImplementedException();
    }

    public void SetCreationTime(string path, IDateTime creationTime)
    {
        throw new NotImplementedException();
    }

    public void SetCreationTimeUtc(string path, IDateTime creationTimeUtc)
    {
        throw new NotImplementedException();
    }

    [DebuggerStepThrough]
    public void SetCurrentDirectory(string path)
    {
        Directory.SetCurrentDirectory(path);
    }

    public void SetLastAccessTime(string path, IDateTime lastAccessTime)
    {
        throw new NotImplementedException();
    }

    public void SetLastAccessTimeUtc(string path, IDateTime lastAccessTimeUtc)
    {
        throw new NotImplementedException();
    }

    public void SetLastWriteTime(string path, IDateTime lastWriteTime)
    {
        throw new NotImplementedException();
    }

    public void SetLastWriteTimeUtc(string path, IDateTime lastWriteTimeUtc)
    {
        throw new NotImplementedException();
    }
}
