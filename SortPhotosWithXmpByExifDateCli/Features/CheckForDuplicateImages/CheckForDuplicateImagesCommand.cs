using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDateCli.Commands;
using SortPhotosWithXmpByExifDateCli.Repository;

namespace SortPhotosWithXmpByExifDateCli.Features.CheckForDuplicateImages;

internal class CheckForDuplicateImagesCommand : FileScannerCommandBase
{
    public CheckForDuplicateImagesCommand(
        ILogger<CommandLine> logger, CommandlineOptions commandlineOptions,
        Func<FileScanner?> getFileScanner, Action<FileScanner> setFileScanner)
        : base(logger, commandlineOptions, getFileScanner, setFileScanner)
    {
    }

    internal override Command GetCommand()
    {
        var checkForDuplicateImagesCommand = new Command(
            "checkForDuplicateImages",
            "Scan for images that are duplicates and remove them.")
        {
            SourceOption,
            SimilarityOption,
            ForceOption,
            MoveOption
        };

        checkForDuplicateImagesCommand.SetHandler(CheckForDuplicateImages!,
        SourceOption,
        ForceOption,
        SimilarityOption,
        MoveOption);

        return checkForDuplicateImagesCommand;
    }

    private void CheckForDuplicateImages(string directory, bool force, int similarity, bool move)
    {
        Run(new CheckForDuplicateImagesRunner(
            Logger, new HashRepository(Logger, 
            Configuration.GetBasePath()), 
            GetFileScanner(directory), 
            force, 
            similarity));
    }
}