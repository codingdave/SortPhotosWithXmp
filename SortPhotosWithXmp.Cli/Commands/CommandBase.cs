using Microsoft.Extensions.Logging;
using System.CommandLine;
using SystemInterface.IO;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.CommandLine;
using SortPhotosWithXmp.Result;
using SortPhotosWithXmp.Features;

namespace SortPhotosWithXmp.Commands;

internal abstract class CommandBase
{
    public ILogger<LoggerContext> Logger { get; }
    private readonly CommandlineOptions _commandlineOptions;

    internal Option<string?> SourceOption => _commandlineOptions.SourceOption;
    internal Option<string?> DestinationOption => _commandlineOptions.DestinationOption;
    internal Option<object?> OffsetOption => _commandlineOptions.OffsetOption;
    internal Option<bool> ForceOption => _commandlineOptions.ForceOption;
    internal Option<bool> MoveOption => _commandlineOptions.MoveOption;
    internal Option<int> SimilarityOption => _commandlineOptions.SimilarityOption;

    public IFile FileWrapper { get; }

    public IDirectory DirectoryWrapper { get; }

    protected CommandBase(
        ILogger<LoggerContext> logger,
        CommandlineOptions commandlineOptions,
        IFile fileWrapper,
        IDirectory directoryWrapper)
    {
        Logger = logger;
        _commandlineOptions = commandlineOptions;
        FileWrapper = fileWrapper;
        DirectoryWrapper = directoryWrapper;
    }

    protected void Run(IRun f)
    {
        try
        {
            var result = f.Run(Logger);

            if (result is FilesFoundResult filesFoundResult)
            {
                Logger.LogInformation($"Processing FilesFoundResult");
                filesFoundResult.Performers.Perform(Logger);
                filesFoundResult.MetaDataErrorPerformer.Perform(Logger);
                filesFoundResult.FileAlreadyExistsErrorPerformer.Perform(Logger);
                filesFoundResult.NoTimeFoundErrorPerformer.Perform(Logger);
                filesFoundResult.DeleteDirectoriesPerformer.Perform(Logger);
            }
            result.Log(Logger);
            Logger.LogInformation($"Done processing result");
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }

    internal abstract Command GetCommand();
}