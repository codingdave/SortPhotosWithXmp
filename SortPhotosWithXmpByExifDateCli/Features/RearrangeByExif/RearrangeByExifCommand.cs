using System.CommandLine;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Operations;
using SortPhotosWithXmpByExifDateCli.Repository;
using SortPhotosWithXmpByExifDateCli.Commands;

namespace SortPhotosWithXmpByExifDateCli.Features.RearrangeByExif;

internal class RearrangeByExifCommand : FileScannerCommandBase
{
    public RearrangeByExifCommand(
        ILogger<CommandLine> logger, CommandlineOptions commandlineOptions,
        Func<FileScanner?> getFileScanner, Action<FileScanner> setFileScanner)
        : base(logger, commandlineOptions, getFileScanner, setFileScanner)
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

    public void RearrangeByExif(string sourcePath, string destinationPath, bool force, bool move)
    {
        var operationPerformer = OperationPerformerFactory.GetCopyOrMovePerformer(Logger, move, force);
        var deleteDirectoryPerformer = new DeleteDirectoryOperation(Logger, force);

        var fileScanner = GetFileScanner(sourcePath);

        Run(new RearrangeByExifRunner(Logger, sourcePath, destinationPath, fileScanner, operationPerformer, deleteDirectoryPerformer));
    }
}