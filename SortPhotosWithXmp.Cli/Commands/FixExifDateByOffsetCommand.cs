using System.CommandLine;

using Microsoft.Extensions.Logging;


using SortPhotosWithXmp.CommandLine;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Features;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Commands;

internal class FixExifDateByOffsetCommand : CommandBase
{
    public FixExifDateByOffsetCommand(
        ILogger<LoggerContext> logger,
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