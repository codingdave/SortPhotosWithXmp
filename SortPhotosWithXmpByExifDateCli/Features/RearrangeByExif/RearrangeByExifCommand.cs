using System.CommandLine;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Repository;
using SortPhotosWithXmpByExifDateCli.Commands;
using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDateCli.Features.RearrangeByExif;

internal class RearrangeByExifCommand : FileScannerCommandBase
{
    public RearrangeByExifCommand(
        ILogger<CommandLine> logger,
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

    private void RearrangeByExif(string sourcePath, string destinationPath, bool force, bool move)
    {
        Run(new RearrangeByExifRunner(
            Logger, 
            sourcePath, 
            destinationPath, 
            GetFileScanner(sourcePath),
            FileWrapper,
            DirectoryWrapper,
            move,
            force));
    }
}