using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Commands;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Features.CheckForDuplicateImages;
using SortPhotosWithXmpByExifDate.Cli.Features.CheckIfFileNameContainsDateDifferentToExifDates;
using SortPhotosWithXmpByExifDate.Cli.Features.DeleteEmptyDirectory;
using SortPhotosWithXmpByExifDate.Cli.Features.DeleteLonelyXmp;
using SortPhotosWithXmpByExifDate.Cli.Features.FixExifDateByOffset;
using SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByCameraManufacturer;
using SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByExif;
using SortPhotosWithXmpByExifDate.Cli.Features.RearrangeBySoftware;
using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli;

internal class CommandLine
{
    private readonly CommandlineOptions _options = new();

    private readonly ILogger<CommandLine> _logger;
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

    public CommandLine()
    {
        var host = Configuration.CreateHost();
        _logger = host.Services.GetRequiredService<ILogger<CommandLine>>();
        var file = host.Services.GetRequiredService<IFile>();
        var directory = host.Services.GetRequiredService<IDirectory>();
        _logger.TestInformationLevels();

        _rootCommand = new RootCommand("Rearrange files containing Exif data")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        _rootCommand.AddCommand(new DeleteEmptyDirectoryCommand(_logger, _options, file, directory).GetCommand());
        _rootCommand.AddCommand(new RearrangeByExifCommand(_logger, _options, file, directory, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new FixExifDateByOffsetCommand(_logger, _options, file, directory).GetCommand());
        _rootCommand.AddCommand(new DeleteLeftoverXmpsCommand(_logger, _options, file, directory, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new CheckForDuplicateImagesCommand(_logger, _options, file, directory, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new CheckIfFileNameContainsDateDifferentToExifDatesCommand(_logger, _options, file, directory).GetCommand());
        _rootCommand.AddCommand(new RearrangeByCameraManufacturerCommand(_logger, _options, file, directory).GetCommand());
        _rootCommand.AddCommand(new RearrangeBySoftwareCommand(_logger, _options, file, directory).GetCommand());
    }

    public async Task<int> InvokeAsync(string[] args)
    {
        var t = _rootCommand.InvokeAsync(args);
        _logger.LogInformation($"Application finished");
        return await t;
    }
}
