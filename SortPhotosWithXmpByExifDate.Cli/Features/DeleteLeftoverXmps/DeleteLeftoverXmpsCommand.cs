using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Commands;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.DeleteLonelyXmp;

internal class DeleteLeftoverXmpsCommand : FileScannerCommandBase
{
    public DeleteLeftoverXmpsCommand(
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
        try
        {
            Run(new DeleteLeftoverXmpsRunner(force, GetFileScanner(sourcePath), File));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}