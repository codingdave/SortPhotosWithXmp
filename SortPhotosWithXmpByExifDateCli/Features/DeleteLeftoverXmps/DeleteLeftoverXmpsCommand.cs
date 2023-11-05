using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDateCli.Commands;
using SortPhotosWithXmpByExifDateCli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDateCli.Features.DeleteLonelyXmp;

internal class DeleteLeftoverXmpsCommand : FileScannerCommandBase
{
    public DeleteLeftoverXmpsCommand(
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
        var command = new Command(
            "deleteLeftoverXmps",
            "Scan for lonely/leftover xmps and remove them.")
        {
            SourceOption,
            ForceOption
        };

        command.SetHandler(DeleteLeftoverXmps!,
            SourceOption!,
            ForceOption!);
        return command;
    }

    private void DeleteLeftoverXmps(string sourcePath, bool force)
    {
        Run(new DeleteLeftoverXmpsRunner(force, GetFileScanner(sourcePath), FileWrapper));
    }
}