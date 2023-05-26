using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal class CommandLine
{
    private static readonly string[] _extensions = new string[]
    {
        ".jpg",
        ".nef",
        ".gif",
        ".png",
        ".psd",
        ".cr3",
        ".arw",
        ".mp4",
        ".mov"
    };

    private readonly Option<DirectoryInfo?> _sourceOption;
    private readonly Option<DirectoryInfo?> _destinationOption;
    private readonly Option<object?> _offsetOption;
    private readonly Option<bool> _forceOption;
    private readonly Option<bool> _moveOption;

    private readonly ILogger<CommandLine> _logger;
    private readonly RootCommand _rootCommand;

    public CommandLine()
    {
        _sourceOption = OptionsHelper.GetSourceOption();
        _destinationOption = OptionsHelper.GetDestinationOption();
        _offsetOption = OptionsHelper.GetOffsetOption();
        _forceOption = OptionsHelper.GetForceOption();
        _moveOption = OptionsHelper.GetMoveOption();
        var host = Host.CreateDefaultBuilder()
        // .ConfigureServices(serviceCollection => { })
        .ConfigureAppConfiguration(configurationBuilder =>
        {
            _ = configurationBuilder
                .SetBasePath(GetBasePath())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureLogging(loggingBuilder =>
        {
            _ = loggingBuilder.ClearProviders();
            _ = Debugger.IsAttached ? loggingBuilder.AddDebug() : loggingBuilder.AddConsole();
        })
        .Build();

        _logger = host.Services.GetRequiredService<ILogger<CommandLine>>();
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
        AddRearrangeByCameraManufacturerCommand();
        AddRearrangeBySoftwareCommand();
        AddFixExifDateByOffsetCommand();
    }

    private static string GetBasePath()
    {
        // using var processModule = Process.GetCurrentProcess().MainModule;
        // return Path.GetDirectoryName(processModule?.FileName) ?? string.Empty;
        // return Directory.GetCurrentDirectory();
        // return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        return System.AppContext.BaseDirectory;
    }

    public async Task<int> InvokeAsync(string[] args)
    {
        return await _rootCommand.InvokeAsync(args);
    }

    private void AddDeleteEmptyDirectoryCommand()
    {
        void DeleteEmptyDirectory(DirectoryInfo directory, bool force)
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
        void SortImagesByExif(DirectoryInfo sourcePath, DirectoryInfo destinationPath, bool force, bool move)
        {
            var operationPerformer = OperationPerformerFactory.GetOperationPerformer(_logger, force, move);
            var deleteDirectoryPerformer = new DeleteDirectoryOperation(_logger, force);

            Run(new SortImageByExif(_logger, sourcePath, destinationPath, _extensions, operationPerformer, deleteDirectoryPerformer));
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
        void CheckIfFileNameContainsDateDifferentToExifDates(DirectoryInfo source, bool force)
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

    private void AddRearrangeByCameraManufacturerCommand()
    {
        void SortImagesByManufacturer(DirectoryInfo source, DirectoryInfo destination, bool force)
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
        void RearrangeBySoftware(DirectoryInfo source, DirectoryInfo destination, bool force)
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

    private void AddFixExifDateByOffsetCommand()
    {
        void FixExifDateByOffset(DirectoryInfo directory, object offset, bool force)
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