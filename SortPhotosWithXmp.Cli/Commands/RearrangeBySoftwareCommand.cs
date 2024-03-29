using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.CommandLine;

using SystemInterface.IO;
using SortPhotosWithXmp.Features;

namespace SortPhotosWithXmp.Commands;

internal class RearrangeBySoftwareCommand : CommandBase
{
    public RearrangeBySoftwareCommand(
        ILogger<LoggerContext> logger,
        CommandlineOptions commandlineOptions,
        IFile fileWrapper,
        IDirectory directoryWrapper)
        : base(logger, commandlineOptions, fileWrapper, directoryWrapper)
    {
    }

    internal override Command GetCommand()
    {
        var command = new Command("rearrangeBySoftware",
            "Find all images of certain application. Prepend software manufacturer to the existing directory structure. Usecase: All F-Spot images might be wrong, enable an easy comparison.")
        {
            SourceOption,
            DestinationOption,
            ForceOption
        };

        command.SetHandler(
            RearrangeBySoftware!,
            SourceOption,
            DestinationOption,
            ForceOption);

        return command;
    }

    private void RearrangeBySoftware(string source, string destination, bool isForce)
    {
        try
        {
            Run(new RearrangeBySoftwareRunner(source, destination, isForce));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}