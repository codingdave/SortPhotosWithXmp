using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDateCli.Commands;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDateCli.Features.DeleteEmptyDirectory;

internal class DeleteEmptyDirectoryCommand : CommandBase
{
    public DeleteEmptyDirectoryCommand(
        ILogger<CommandLine> logger,
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

    private void DeleteEmptyDirectory(string directory, bool force)
    {
        Run(new DeleteEmptyDirectoryRunner(DirectoryWrapper, directory, force));
    }
}