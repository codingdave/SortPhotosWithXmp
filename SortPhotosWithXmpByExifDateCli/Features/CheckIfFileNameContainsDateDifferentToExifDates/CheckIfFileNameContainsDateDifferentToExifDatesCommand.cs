using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDateCli.Commands;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDateCli.Features.CheckIfFileNameContainsDateDifferentToExifDates;

internal class CheckIfFileNameContainsDateDifferentToExifDatesCommand : CommandBase
{
    public CheckIfFileNameContainsDateDifferentToExifDatesCommand(
        ILogger<CommandLine> logger, 
        CommandlineOptions commandlineOptions,
        IFile fileWrapper,
        IDirectory directoryWraper) 
        : base(logger, commandlineOptions, fileWrapper, directoryWraper)
    {
    }

    internal override Command GetCommand()
    {
        var command = new Command("checkIfFileNameContainsDateDifferentToExifDates", 
        "Compare the date in the filename with the date in the file")
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
        Run(new CheckIfFileNameContainsDateDifferentToExifDatesRunner(directory, force));
    }
}