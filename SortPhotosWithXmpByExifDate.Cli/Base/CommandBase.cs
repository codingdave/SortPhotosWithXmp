using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.Statistics;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using System.CommandLine;
using SystemInterface.IO;
using SystemWrapper.IO;

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
            var statistics = f.Run(Logger);

            Logger.LogInformation($"Processing statistics");

            if (statistics is IFoundStatistics filesFoundStatistics)
            {
                statistics.FileErrors.HandleErrorFiles(Logger, filesFoundStatistics, File, Directory);
            }
            statistics.Log();

            Logger.LogInformation($"Done processing statistics");
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }

    internal abstract Command GetCommand();
}