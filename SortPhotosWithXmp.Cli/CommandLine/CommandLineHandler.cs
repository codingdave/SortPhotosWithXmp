using System.CommandLine;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Commands;
using SortPhotosWithXmp.Features;

using SystemInterface.IO;

namespace SortPhotosWithXmp.CommandLine;

internal class CommandLineHandler
{
    private readonly CommandlineOptions _options = new();

    private readonly ILogger<LoggerContext> _logger;
    private readonly RootCommand _rootCommand;
    private FileScanner? _fileScanner;

    public FileScanner? GetFileScanner()
    {
        return _fileScanner;
    }

    public void SetFileScanner(FileScanner value)
    {
        _fileScanner = value;
    }

    public CommandLineHandler(ILogger<LoggerContext> logger, IFile fileWrapper, IDirectory directoryWrapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (fileWrapper is null)
        {
            throw new ArgumentNullException(nameof(fileWrapper));
        }

        if (directoryWrapper is null)
        {
            throw new ArgumentNullException(nameof(directoryWrapper));
        }

        _rootCommand = new RootCommand("Rearrange files containing Exif data")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        _rootCommand.AddCommand(new DeleteEmptyDirectoryCommand(_logger, _options, fileWrapper, directoryWrapper).GetCommand());
        _rootCommand.AddCommand(new RearrangeByExifCommand(_logger, _options, fileWrapper, directoryWrapper, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new FixExifDateByOffsetCommand(_logger, _options, fileWrapper, directoryWrapper).GetCommand());
        _rootCommand.AddCommand(new DeleteLeftoverXmpsCommand(_logger, _options, fileWrapper, directoryWrapper, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new CheckForDuplicateImagesCommand(_logger, _options, fileWrapper, directoryWrapper, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new CheckIfFileNameContainsDateDifferentToExifDatesCommand(_logger, _options, fileWrapper, directoryWrapper).GetCommand());
        _rootCommand.AddCommand(new RearrangeByCameraManufacturerCommand(_logger, _options, fileWrapper, directoryWrapper).GetCommand());
        _rootCommand.AddCommand(new RearrangeBySoftwareCommand(_logger, _options, fileWrapper, directoryWrapper).GetCommand());
    }

    public async Task<int> InvokeAsync(string[] args)
    {
        var t = _rootCommand.InvokeAsync(args);
        _logger.LogInformation($"Application finished");
        return await t;
    }
}