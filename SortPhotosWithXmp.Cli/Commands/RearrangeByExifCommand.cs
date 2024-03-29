using System.CommandLine;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmp.CommandLine;
using SystemInterface.IO;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Features;

namespace SortPhotosWithXmp.Commands;

internal class RearrangeByExifCommand : FileScannerCommandBase
{
    public RearrangeByExifCommand(
        ILogger<LoggerContext> logger,
        CommandlineOptions commandlineOptions,
        IFile fileWrapper,
        IDirectory directoryWrapper,
        Func<FileScanner?> getFileScanner, Action<FileScanner> setFileScanner)
        : base(logger, commandlineOptions, fileWrapper, directoryWrapper, getFileScanner, setFileScanner)
    {
    }

    internal override Command GetCommand()
    {
        var command = new Command("rearrangeByExif",
     "Scan source dir and move photos and videos to destination directory. Create subdirectory structure yyyy/MM/dd given by the Exif information of the image/video source. Xmp files are placed accordingly.")
        {
            SourceOption,
            DestinationOption,
            ForceOption,
            MoveOption
        };

        command.SetHandler(
            RearrangeByExif!,
            SourceOption,
            DestinationOption,
            ForceOption,
            MoveOption);

        return command;
    }

    private void RearrangeByExif(string sourcePath, string destinationPath, bool isForce, bool isMove)
    {
        try
        {
            Run(new RearrangeByExifRunner(
               Logger,
               sourcePath,
               destinationPath,
               GetFileScanner(sourcePath),
               FileWrapper,
               DirectoryWrapper,
               isMove,
               isForce));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}