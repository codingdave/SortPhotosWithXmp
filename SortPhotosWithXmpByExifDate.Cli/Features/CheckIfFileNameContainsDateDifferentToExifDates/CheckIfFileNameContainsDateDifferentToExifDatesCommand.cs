using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Commands;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.CheckIfFileNameContainsDateDifferentToExifDates;

internal class CheckIfFileNameContainsDateDifferentToExifDatesCommand : CommandBase
{
    public CheckIfFileNameContainsDateDifferentToExifDatesCommand(
        ILogger<CommandLine> logger,
        CommandlineOptions commandlineOptions,
        IFile file,
        IDirectory directoryWraper)
        : base(logger, commandlineOptions, file, directoryWraper)
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

    private void DeleteEmptyDirectory(string directory, bool isForce)
    {
        try
        {
            Run(new CheckIfFileNameContainsDateDifferentToExifDatesRunner(directory, isForce));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}