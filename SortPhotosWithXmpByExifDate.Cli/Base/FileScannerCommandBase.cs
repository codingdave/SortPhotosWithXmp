using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Commands;

internal abstract class FileScannerCommandBase : CommandBase
{
    private readonly Func<FileScanner?> _getFileScanner;
    private readonly Action<FileScanner> _setFileScanner;

    public FileScannerCommandBase(
        ILogger<CommandLine> logger,
        CommandlineOptions commandlineOptions,
        IFile file,
        IDirectory directory,
        Func<FileScanner?> getFileScanner, Action<FileScanner> setFileScanner)
        : base(logger, commandlineOptions, file, directory)
    {
        _getFileScanner = getFileScanner;
        _setFileScanner = setFileScanner;
    }

    protected FileScanner GetFileScanner(string sourcePath)
    {
        var fileScanner = _getFileScanner();
        if (fileScanner == null)
        {
            fileScanner = new FileScanner(Logger);
            DirectoryWrapper.SetCurrentDirectory(sourcePath);
            fileScanner.Crawl(DirectoryWrapper);
            _setFileScanner(fileScanner);
        }
        else if (!sourcePath.Equals(fileScanner.ScanDirectory))
        {
            throw new InvalidOperationException($"Previous operation was targeting directory {fileScanner.ScanDirectory}, now we are working on {sourcePath}.");
        }

        return fileScanner ?? throw new InvalidOperationException("Could not create FileScanner");
    }
}
