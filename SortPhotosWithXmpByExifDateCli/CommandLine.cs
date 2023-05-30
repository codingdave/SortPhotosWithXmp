using System.CommandLine;
using System.ComponentModel;
using System.Security.Cryptography;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal class CommandLine
{
    private static readonly string[] _extensions = new string[]
    {
        ".jpg",
        ".jpeg",
        ".nef",
        ".gif",
        ".png",
        ".psd",
        ".cr3",
        ".arw",
        ".mp4",
        ".mov"
    };

    private readonly Option<string?> _sourceOption;
    private readonly Option<string?> _destinationOption;
    private readonly Option<object?> _offsetOption;
    private readonly Option<bool> _forceOption;
    private readonly Option<bool> _moveOption;
    private readonly Option<int> _similarityOption;

    private readonly ILogger<CommandLine> _logger;
    private readonly RootCommand _rootCommand;

    public CommandLine()
    {
        _sourceOption = OptionsHelper.GetSourceOption();
        _destinationOption = OptionsHelper.GetDestinationOption();
        _offsetOption = OptionsHelper.GetOffsetOption();
        _forceOption = OptionsHelper.GetForceOption();
        _moveOption = OptionsHelper.GetMoveOption();
        _similarityOption = OptionsHelper.GetSimilarityOption();
        IHost host = Configuration.CreateHost();
        _logger = host.Services.GetRequiredService<ILogger<CommandLine>>();
        _logger.LogInformation($"BasePath: {Configuration.GetBasePath()}");
        _logger.Log(LogLevel.Trace, "Trace messages will show up");
        _logger.Log(LogLevel.Debug, "Debug messages will show up");
        _logger.Log(LogLevel.Information, "Information messages will show up");
        _logger.Log(LogLevel.Warning, "Warning messages will show up");
        _logger.Log(LogLevel.Error, "Error messages will show up");
        _logger.Log(LogLevel.Critical, "Critical messages will show up");
        _logger.Log(LogLevel.None, "None messages will show up");

        _rootCommand = new RootCommand("Rearrange files containing Exif data")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        AddDeleteEmptyDirectoryCommand();
        AddRearrangeByExifCommand();
        AddCheckIfFileNameContainsDateDifferentToExifDatesCommand();
        AddFixExifDateByOffsetCommand();
        AddDeleteLonelyXmpCommand();
        AddCheckForDuplicateImagesCommand();
        AddRearrangeByCameraManufacturerCommand();
        AddRearrangeBySoftwareCommand();
    }
    public async Task<int> InvokeAsync(string[] args)
    {
        return await _rootCommand.InvokeAsync(args);
    }

    private void AddDeleteEmptyDirectoryCommand()
    {
        void DeleteEmptyDirectory(string directory, bool force)
        {
            Run(new DeleteEmptyDirectory(directory, force));
        }

        var deleteEmptyDirectoryCommand = new Command("deleteEmptyDirectory", "Search recursively for emtpy directories and delete them.")
        {
            _sourceOption,
            _forceOption
        };

        deleteEmptyDirectoryCommand.SetHandler(DeleteEmptyDirectory!, _sourceOption, _forceOption);

        _rootCommand.AddCommand(deleteEmptyDirectoryCommand);
    }

    private void AddRearrangeByExifCommand()
    {
        void SortImagesByExif(string sourcePath, string destinationPath, bool force, bool move)
        {
            var operationPerformer = OperationPerformerFactory.GetOperationPerformer(_logger, force, move);
            var deleteDirectoryPerformer = new DeleteDirectoryOperation(_logger, force);

            Run(new SortImageByExif.SortImageByExifRunner(_logger, sourcePath, destinationPath, _extensions, operationPerformer, deleteDirectoryPerformer));
        }

        var rearrangeByExifCommand = new Command("rearrangeByExif",
            "Scan source dir and move photos and videos to destination directory. Create subdirectory structure yyyy/MM/dd given by the Exif information of the image/video source. Xmp files are placed accordingly.")
        {
            _sourceOption,
            _destinationOption,
            _forceOption,
            _moveOption
        };

        rearrangeByExifCommand.SetHandler(SortImagesByExif!, _sourceOption, _destinationOption, _forceOption, _moveOption);

        _rootCommand.AddCommand(rearrangeByExifCommand);
    }

    private void AddCheckIfFileNameContainsDateDifferentToExifDatesCommand()
    {
        void CheckIfFileNameContainsDateDifferentToExifDates(string source, bool force)
        {
            Run(new CheckIfFileNameContainsDateDifferentToExifDates(source, force));
        }

        var checkIfFileNameContainsDateDifferentToExifDatesCommand = new Command(
            "checkIfFileNameContainsDateDifferentToExifDates",
            "There might be situations in which the filename contains the correct timecheck if image timestamp differs from exif and rename file.")
        {
            _sourceOption,
            _forceOption
        };

        checkIfFileNameContainsDateDifferentToExifDatesCommand.SetHandler(
            CheckIfFileNameContainsDateDifferentToExifDates!, _sourceOption, _forceOption);

        _rootCommand.AddCommand(checkIfFileNameContainsDateDifferentToExifDatesCommand);
    }


    private void AddFixExifDateByOffsetCommand()
    {
        void FixExifDateByOffset(string directory, object offset, bool force)
        {
            // https://github.com/dotnet/command-line-api/issues/2086
            Run(new FixExifDateByOffset(directory, (TimeSpan)offset, force));
        }

        var fixExifDateByOffsetCommand = new Command(
            "fixExifDateByOffset",
            "Fix their exif by identifying the offset.")
        {
            _sourceOption,
            _offsetOption,
            _forceOption
        };

        fixExifDateByOffsetCommand.SetHandler(FixExifDateByOffset!, _sourceOption!, _offsetOption!, _forceOption);

        _rootCommand.AddCommand(fixExifDateByOffsetCommand);
    }

    private void AddDeleteLonelyXmpCommand()
    {
        void DeleteLonelyXmps(string directory, bool force)
        {
            // https://github.com/dotnet/command-line-api/issues/2086
            // Run(new FixExifDateByOffset(directory, (TimeSpan)offset, force));
            throw new NotImplementedException();
        }

        var deleteLeftoverXmpsCommand = new Command(
            "deleteLeftoverXmpsCommand",
            "Scan for lonely/leftover xmps and remove them.")
        {
            _sourceOption,
            _forceOption
        };

        deleteLeftoverXmpsCommand.SetHandler(DeleteLonelyXmps!, _sourceOption!, _forceOption);

        _rootCommand.AddCommand(deleteLeftoverXmpsCommand);
    }

    private void AddCheckForDuplicateImagesCommand()
    {
        void CheckForDuplicateImages(string directory, bool force, int similarity)
        {
            Run(new CheckForDuplicates.CheckForDuplicatesRunner(_logger, directory, force, similarity));
        }

        var checkForDuplicateImagesCommand = new Command(
            "checkForDuplicateImagesCommand",
            "Scan for images that are duplicates and remove them.")
        {
            _sourceOption,
            _similarityOption,
            _forceOption
        };

        checkForDuplicateImagesCommand.SetHandler(CheckForDuplicateImages!, _sourceOption, _forceOption, _similarityOption);

        _rootCommand.AddCommand(checkForDuplicateImagesCommand);
    }

    private void AddRearrangeByCameraManufacturerCommand()
    {
        void SortImagesByManufacturer(string source, string destination, bool force)
        {
            Run(new SortImagesByManufacturer(source, destination, force));
        }

        var rearrangeByCameraManufacturerCommand = new Command("rearrangeByCameraManufacturer",
            "Find all images of certain camera. Prepend camera manufacturer to the existing directory structure.")
        {
            _sourceOption,
            _destinationOption,
            _forceOption
        };

        rearrangeByCameraManufacturerCommand.SetHandler(SortImagesByManufacturer!, _sourceOption, _destinationOption, _forceOption);

        _rootCommand.AddCommand(rearrangeByCameraManufacturerCommand);
    }

    private void AddRearrangeBySoftwareCommand()
    {
        void RearrangeBySoftware(string source, string destination, bool force)
        {
            Run(new RearrangeBySoftware(source, destination, force));
        }

        var rearrangeBySoftwareCommand = new Command("rearrangeBySoftware",
            "Find all images of certain application. Prepend software manufacturer to the existing directory structure. Usecase: All F-Spot images might be wrong, enable an easy comparison.")
        {
            _sourceOption,
            _destinationOption,
            _forceOption
        };

        rearrangeBySoftwareCommand.SetHandler(RearrangeBySoftware!, _sourceOption, _destinationOption, _forceOption);

        _rootCommand.AddCommand(rearrangeBySoftwareCommand);
    }

    private void Run(IRun f)
    {
        try
        {
            var statistics = f.Run(_logger);
            if (statistics is IFoundStatistics filesFoundStatistics)
            {
                statistics.FileErrors.HandleErrorFiles(_logger, filesFoundStatistics);
            }
            statistics.Log();
        }
        catch (Exception e)
        {
            _logger.LogExceptionError(e);
        };
    }
}