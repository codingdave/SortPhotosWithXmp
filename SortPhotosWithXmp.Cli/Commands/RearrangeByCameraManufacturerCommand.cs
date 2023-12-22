using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.CommandLine;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Features;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Commands;
internal class RearrangeByCameraManufacturerCommand : CommandBase
{
    public RearrangeByCameraManufacturerCommand(
        ILogger<LoggerContext> logger,
        CommandlineOptions commandlineOptions,
        IFile file,
        IDirectory directory)
        : base(logger, commandlineOptions, file, directory)
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

    private void RearrangeByCameraManufacturer(string source, string destination, bool isForce)
    {
        try
        {
            Run(new RearrangeByCameraManufacturerRunner(source, destination, isForce));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}