using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.CommandLine;

using SystemInterface.IO;
using SortPhotosWithXmp.Features;

namespace SortPhotosWithXmp.Commands;

internal class CheckIfFileNameContainsDateDifferentToExifDatesCommand : CommandBase
{
    public CheckIfFileNameContainsDateDifferentToExifDatesCommand(
        ILogger<LoggerContext> logger,
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