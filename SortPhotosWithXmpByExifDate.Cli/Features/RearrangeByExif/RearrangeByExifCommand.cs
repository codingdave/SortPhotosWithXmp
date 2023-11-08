using System.CommandLine;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Commands;
using SystemInterface.IO;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByExif;

internal class RearrangeByExifCommand : FileScannerCommandBase
{
    public RearrangeByExifCommand(
        ILogger<CommandLine> logger,
        CommandlineOptions commandlineOptions,
        IFile file,
        IDirectory directory,
        Func<FileScanner?> getFileScanner, Action<FileScanner> setFileScanner)
        : base(logger, commandlineOptions, file, directory, getFileScanner, setFileScanner)
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

    private void RearrangeByExif(string sourcePath, string destinationPath, bool force, bool isMove)
    {
        try
        {
            Run(new RearrangeByExifRunner(
               Logger,
               sourcePath,
               destinationPath,
               GetFileScanner(sourcePath),
               File,
               Directory,
               isMove,
               force));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}