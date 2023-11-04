using System.CommandLine;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Commands;

namespace SortPhotosWithXmpByExifDateCli.Features.FixExifDateByOffset;

internal class FixExifDateByOffsetCommand : CommandBase
{
    public FixExifDateByOffsetCommand(ILogger<CommandLine> logger, CommandlineOptions commandlineOptions)
    : base(logger, commandlineOptions) { }

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

    public void FixExifDateByOffset(string directory, object offset, bool force)
    {
        // https://github.com/dotnet/command-line-api/issues/2086
        Run(new FixExifDateByOffsetRunner(directory, (TimeSpan)offset, force));
    }
}