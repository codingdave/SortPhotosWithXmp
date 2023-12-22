using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Extensions;
using SortPhotosWithXmpByExifDate.CommandLine;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Features.CheckIfFileNameContainsDateDifferentToExifDates;

internal class CheckIfFileNameContainsDateDifferentToExifDatesCommand : CommandBase
{
    public CheckIfFileNameContainsDateDifferentToExifDatesCommand(
        ILogger<CommandLineHandler> logger,
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