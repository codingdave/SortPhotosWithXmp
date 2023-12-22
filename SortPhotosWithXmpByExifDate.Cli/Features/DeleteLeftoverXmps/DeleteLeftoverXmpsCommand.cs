using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.CommandLine;using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Extensions;
using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.DeleteLonelyXmp;

#warning Rename Xmp to SidecarFiles or the other way around

internal class DeleteLeftoverXmpsCommand : FileScannerCommandBase
{
    public DeleteLeftoverXmpsCommand(
        ILogger<CommandLineHandler> logger,
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

    private void DeleteLeftoverXmps(string sourcePath, bool isForce)
    {
        try
        {
            Run(new DeleteLeftoverXmpsRunner(isForce, GetFileScanner(sourcePath), File));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}