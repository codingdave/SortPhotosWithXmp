using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.CommandLine;
using SortPhotosWithXmp.Features;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Commands;

internal abstract class FileScannerCommandBase : CommandBase
{
    private readonly Func<FileScanner?> _getFileScanner;
    private readonly Action<FileScanner> _setFileScanner;

    public FileScannerCommandBase(
        ILogger<LoggerContext> logger,
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
        Logger.LogInformation("FileScanner requested");
        var fileScanner = _getFileScanner();
        if (fileScanner == null)
        {
            fileScanner = new FileScanner(Logger, FileWrapper);
            DirectoryWrapper.SetCurrentDirectory(sourcePath);
            fileScanner.Crawl(DirectoryWrapper);
            _setFileScanner(fileScanner);
            Logger.LogInformation($"New FileScanner has been created for {sourcePath}");
        }
        else if (!sourcePath.Equals(fileScanner.ScanDirectory))
        {
            throw new InvalidOperationException($"Previous operation was targeting directory {fileScanner.ScanDirectory}, now we are working on {sourcePath}.");
        }
        else
        {
            Logger.LogInformation("Reusing existing FileScanner");
        }

        return fileScanner ?? throw new InvalidOperationException("Could not create FileScanner");
    }
}
