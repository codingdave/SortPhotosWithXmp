using System.CommandLine;

using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.Commands;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Extensions;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.FixExifDateByOffset;

internal class FixExifDateByOffsetCommand : CommandBase
{
    public FixExifDateByOffsetCommand(
        ILogger<CommandLine> logger,
        CommandlineOptions commandlineOptions,
        IFile file,
        IDirectory directory)
    : base(logger, commandlineOptions, file, directory) { }

    internal override Command GetCommand()
    {
        var command = new Command(
            "fixExifDateByOffset",
            "Fix their exif by identifying the offset.")
        {
            SourceOption,
            OffsetOption,
            ForceOption
        };

        command.SetHandler(
            FixExifDateByOffset!,
            SourceOption!,
            OffsetOption!,
            ForceOption);

        return command;
    }

    private void FixExifDateByOffset(string directory, object offset, bool isForce)
    {
        try
        {
            // https://github.com/dotnet/command-line-api/issues/2086
            Run(new FixExifDateByOffsetRunner(directory, (TimeSpan)offset, isForce));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}