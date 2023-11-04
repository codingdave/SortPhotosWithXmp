using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDateCli.Commands;
using SortPhotosWithXmpByExifDateCli.Repository;

namespace SortPhotosWithXmpByExifDateCli.Features.DeleteLonelyXmp;

internal class DeleteLonelyXmpCommand : FileScannerCommandBase
{
    public DeleteLonelyXmpCommand(
        ILogger<CommandLine> logger, CommandlineOptions commandlineOptions,
        Func<FileScanner?> getFileScanner, Action<FileScanner> setFileScanner)
        : base(logger, commandlineOptions, getFileScanner, setFileScanner)
    {
    }

    internal override Command GetCommand()
    {
        var deleteLeftoverXmpsCommand = new Command(
            "deleteLeftoverXmps",
            "Scan for lonely/leftover xmps and remove them.")
        {
            SourceOption,
            ForceOption
        };

        deleteLeftoverXmpsCommand.SetHandler(DeleteLonelyXmps!,
            SourceOption!,
            ForceOption!);
        return deleteLeftoverXmpsCommand;
    }

    private void DeleteLonelyXmps(string sourcePath, bool force)
    {
        Run(new DeleteLonelyXmpRunner(force, GetFileScanner(sourcePath)));
    }
}