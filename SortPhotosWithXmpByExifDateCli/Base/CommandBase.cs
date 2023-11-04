using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;
using System.CommandLine;
using SystemInterface.IO;
using SystemWrapper.IO;

namespace SortPhotosWithXmpByExifDateCli.Commands;

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

    public IFile FileWrapper { get; }

    public IDirectory DirectoryWrapper { get; }


    protected CommandBase(

        ILogger<CommandLine> logger,
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
            var statistics = f.Run(Logger);
            if (statistics is IFoundStatistics filesFoundStatistics)
            {
                statistics.FileErrors.HandleErrorFiles(Logger, filesFoundStatistics, new FileWrap());
            }
            statistics.Log();
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }

    internal abstract Command GetCommand();
}