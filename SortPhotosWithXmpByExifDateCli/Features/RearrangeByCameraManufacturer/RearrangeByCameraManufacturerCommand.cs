using System.CommandLine;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Commands;

namespace SortPhotosWithXmpByExifDateCli.Features.RearrangeByCameraManufacturer;

internal class RearrangeByCameraManufacturerCommand : CommandBase
{
    public RearrangeByCameraManufacturerCommand(
        ILogger<CommandLine> logger, CommandlineOptions commandlineOptions)
        : base(logger, commandlineOptions)
    {
    }

    internal override Command GetCommand()
    {
        var command = new Command("rearrangeByCameraManufacturer",
            "Find all images of certain camera. Prepend camera manufacturer to the existing directory structure.")
        {
            SourceOption,
            DestinationOption,
            ForceOption
        };

        command.SetHandler(
            RearrangeByCameraManufacturer!,
            SourceOption,
            DestinationOption,
            ForceOption);

        return command;
    }

    public void RearrangeByCameraManufacturer(string source, string destination, bool force)
    {
        Run(new RearrangeByCameraManufacturerRunner(source, destination, force));
    }
}