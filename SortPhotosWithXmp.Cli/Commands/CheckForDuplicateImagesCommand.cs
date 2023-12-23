using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.CommandLine;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Features;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Commands;

internal class CheckForDuplicateImagesCommand : FileScannerCommandBase
{
    public CheckForDuplicateImagesCommand(
        ILogger<LoggerContext> logger,
        CommandlineOptions commandlineOptions,
        IFile fileWrapper,
        IDirectory directoryWrapper,
        Func<FileScanner?> getFileScanner, Action<FileScanner> setFileScanner)
        : base(logger, commandlineOptions, fileWrapper, directoryWrapper, getFileScanner, setFileScanner)
    {
    }

    internal override Command GetCommand()
    {
        var command = new Command(
            "checkForDuplicateImages",
            "Scan for images that are duplicates and remove them.")
        {
            SourceOption,
            SimilarityOption,
            ForceOption,
            MoveOption
        };

        command.SetHandler(CheckForDuplicateImages!,
        SourceOption,
        ForceOption,
        SimilarityOption,
        MoveOption);

        return command;
    }

    private void CheckForDuplicateImages(string directory, bool isForce, int similarity, bool move)
    {
        try
        {
            Run(new CheckForDuplicateImagesRunner(
             Logger,
             GetFileScanner(directory),
             isForce,
             similarity));
        }
        catch (Exception e)
        {
            Logger.LogExceptionError(e);
        }
    }
}