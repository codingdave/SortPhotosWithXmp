using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.Result;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using System.CommandLine;
using SystemInterface.IO;
using SortPhotosWithXmpByExifDate.Cli.Extensions;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using Microsoft.VisualBasic;

namespace SortPhotosWithXmpByExifDate.Cli.Commands;

internal abstract class CommandBase
{
    public ILogger<CommandLine> Logger { get; }
    private readonly CommandlineOptions _commandlineOptions;

    internal Option<string?> SourceOption => _commandlineOptions.SourceOption;
    internal Option<string?> DestinationOption => _commandlineOptions.DestinationOption;
    internal Option<object?> OffsetOption => _commandlineOptions.OffsetOption;
    internal Option<bool> ForceOption => _commandlineOptions.ForceOption;
    internal Option<bool> MoveOption => _commandlineOptions.MoveOption;
    internal Option<int> SimilarityOption => _commandlineOptions.SimilarityOption;

    public IFile File { get; }

    public IDirectory Directory { get; }

    protected CommandBase(
        ILogger<CommandLine> logger,
        CommandlineOptions commandlineOptions,
        IFile file,
        IDirectory directory)
    {
        Logger = logger;
        _commandlineOptions = commandlineOptions;
        File = file;
        Directory = directory;
    }

    protected void Run(IRun f)
    {
        try
        {
            var result = f.Run(Logger);

            if (result is FilesFoundResult filesFoundResult)
            {
                Logger.LogInformation($"Processing FilesFoundResult");
                filesFoundResult.PerformerCollection.Perform(Logger);
                filesFoundResult.MetaDataErrorPerformer.Perform(Logger);
                filesFoundResult.FileAlreadyExistsErrorPerformer.Perform(Logger);
                filesFoundResult.NoTimeFoundErrorPerformer.Perform(Logger);
                filesFoundResult.CleanupPerformer.Perform(Logger);
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