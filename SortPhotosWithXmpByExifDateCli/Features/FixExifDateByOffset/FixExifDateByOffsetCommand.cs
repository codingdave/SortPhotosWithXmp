using System.CommandLine;

using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDateCli.Commands;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDateCli.Features.FixExifDateByOffset;

internal class FixExifDateByOffsetCommand : CommandBase
{
    public FixExifDateByOffsetCommand(
        ILogger<CommandLine> logger, 
        CommandlineOptions commandlineOptions,
        IFile fileWrapper,
        IDirectory directoryWrapper)
    : base(logger, commandlineOptions, fileWrapper, directoryWrapper) { }

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

    private void FixExifDateByOffset(string directory, object offset, bool force)
    {
        // https://github.com/dotnet/command-line-api/issues/2086
        Run(new FixExifDateByOffsetRunner(directory, (TimeSpan)offset, force));
    }
}