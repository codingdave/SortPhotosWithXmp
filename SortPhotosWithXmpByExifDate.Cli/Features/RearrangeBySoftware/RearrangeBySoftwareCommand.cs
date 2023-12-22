using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Extensions;
using SortPhotosWithXmpByExifDate.CommandLine;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Features.RearrangeBySoftware;

internal class RearrangeBySoftwareCommand : CommandBase
{
    public RearrangeBySoftwareCommand(
        ILogger<CommandLineHandler> logger,
        CommandlineOptions commandlineOptions,
        IFile file,
        IDirectory directory)
        : base(logger, commandlineOptions, file, directory)
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