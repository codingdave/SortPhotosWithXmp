using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDateCli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDateCli.Commands;

internal abstract class FileScannerCommandBase : CommandBase
{
    private readonly Func<FileScanner?> _getFileScanner;
    private readonly Action<FileScanner> _setFileScanner;

    public FileScannerCommandBase(
        ILogger<CommandLine> logger,
        CommandlineOptions commandlineOptions,
        IFile fileWrapper,
        IDirectory directoryWrapper,
        Func<FileScanner?> getFileScanner, Action<FileScanner> setFileScanner)
        : base(logger, commandlineOptions, fileWrapper, directoryWrapper)
    {
        _getFileScanner = getFileScanner;
        _setFileScanner = setFileScanner;
    }

    protected FileScanner GetFileScanner(string sourcePath)
    {
        var fileScanner = _getFileScanner();
        if (fileScanner == null)
        {
            fileScanner = new FileScanner(Logger, sourcePath);
            _setFileScanner(fileScanner);
        }
        else if (!fileScanner.ScanDirectory.Equals(sourcePath))
        {
            throw new InvalidOperationException($"Previous operation was targeting directory {fileScanner.ScanDirectory}, now we are working on {sourcePath}.");
        }

        return fileScanner ?? throw new InvalidOperationException("Could not create FileScanner");
    }
}
