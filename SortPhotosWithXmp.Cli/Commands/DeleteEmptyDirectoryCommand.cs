using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.CommandLine;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Features;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Commands;

internal class DeleteEmptyDirectoryCommand : CommandBase
{
    public DeleteEmptyDirectoryCommand(
        ILogger<LoggerContext> logger,
        CommandlineOptions commandlineOptions,
        IFile fileWrapper,
        IDirectory directoryWrapper)
        : base(logger, commandlineOptions, fileWrapper, directoryWrapper)
    {
    }

    internal override Command GetCommand()
    {
        var command = new Command("deleteEmptyDirectory", "Search recursively for emtpy directories and delete them.")
        {
            SourceOption,
            ForceOption
        };

        command.SetHandler(
            DeleteEmptyDirectory!,
            SourceOption,
            ForceOption);

        return command;
    }

    private void DeleteEmptyDirectory(string directory, bool isForce)
    {
        try
        {
            Run(new DeleteEmptyDirectoryRunner(DirectoryWrapper, directory, isForce));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}