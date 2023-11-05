using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Commands;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.DeleteEmptyDirectory;

internal class DeleteEmptyDirectoryCommand : CommandBase
{
    public DeleteEmptyDirectoryCommand(
        ILogger<CommandLine> logger,
        CommandlineOptions commandlineOptions,
        IFile file,
        IDirectory directory)
        : base(logger, commandlineOptions, file, directory)
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