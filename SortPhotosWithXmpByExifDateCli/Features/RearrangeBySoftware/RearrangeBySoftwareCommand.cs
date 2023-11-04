using System.CommandLine;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Commands;

namespace SortPhotosWithXmpByExifDateCli.Features.RearrangeBySoftware;

internal class RearrangeBySoftwareCommand : CommandBase
{
    public RearrangeBySoftwareCommand(
        ILogger<CommandLine> logger, CommandlineOptions commandlineOptions)
        : base(logger, commandlineOptions)
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

    private void RearrangeBySoftware(string source, string destination, bool force)
    {
        Run(new RearrangeBySoftwareRunner(source, destination, force));
    }
}