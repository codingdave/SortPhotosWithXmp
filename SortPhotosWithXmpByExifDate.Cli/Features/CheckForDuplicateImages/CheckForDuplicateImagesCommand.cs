using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Commands;
using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.CheckForDuplicateImages;

internal class CheckForDuplicateImagesCommand : FileScannerCommandBase
{
    public CheckForDuplicateImagesCommand(
        ILogger<CommandLine> logger,
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

    private void CheckForDuplicateImages(string directory, bool force, int similarity, bool move)
    {
        Run(new CheckForDuplicateImagesRunner(
            Logger,
            GetFileScanner(directory),
            force,
            similarity));
    }
}