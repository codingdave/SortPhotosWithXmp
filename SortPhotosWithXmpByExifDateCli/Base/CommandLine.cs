using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Commands;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;
using SortPhotosWithXmpByExifDateCli.Features.CheckForDuplicateImages;
using SortPhotosWithXmpByExifDateCli.Features.CheckIfFileNameContainsDateDifferentToExifDates;
using SortPhotosWithXmpByExifDateCli.Features.DeleteEmptyDirectory;
using SortPhotosWithXmpByExifDateCli.Features.DeleteLonelyXmp;
using SortPhotosWithXmpByExifDateCli.Features.FixExifDateByOffset;
using SortPhotosWithXmpByExifDateCli.Features.RearrangeByCameraManufacturer;
using SortPhotosWithXmpByExifDateCli.Features.RearrangeByExif;
using SortPhotosWithXmpByExifDateCli.Features.RearrangeBySoftware;
using SortPhotosWithXmpByExifDateCli.Repository;

namespace SortPhotosWithXmpByExifDateCli;

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
        _logger.TestInformationLevels();

        _rootCommand = new RootCommand("Rearrange files containing Exif data")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        _rootCommand.AddCommand(new DeleteEmptyDirectoryCommand(_logger, _options).GetCommand());
        _rootCommand.AddCommand(new RearrangeByExifCommand(_logger, _options, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new FixExifDateByOffsetCommand(_logger, _options).GetCommand());
        _rootCommand.AddCommand(new DeleteLonelyXmpCommand(_logger, _options, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new CheckForDuplicateImagesCommand(_logger, _options, GetFileScanner, SetFileScanner).GetCommand());
        _rootCommand.AddCommand(new CheckIfFileNameContainsDateDifferentToExifDatesCommand(_logger, _options).GetCommand());
        _rootCommand.AddCommand(new RearrangeByCameraManufacturerCommand(_logger, _options).GetCommand());
        _rootCommand.AddCommand(new RearrangeBySoftwareCommand(_logger, _options).GetCommand());
    }

    public async Task<int> InvokeAsync(string[] args)
    {
        return await _rootCommand.InvokeAsync(args);
    }
}
