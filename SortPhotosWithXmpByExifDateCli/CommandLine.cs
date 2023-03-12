using System.CommandLine;

namespace SortPhotosWithXmpByExifDateCli;

internal class CommandLine
{
    private static IEnumerable<string> _extensions = new List<string>()
    {
        ".jpg",
        ".nef",
        ".gif",
        ".mp4",
        ".png",
        ".cr3"
    };

    private Option<DirectoryInfo?> _sourceOption;
    private Option<DirectoryInfo?> _destinationOption;
    private Option<object?> _offsetOption;

    public CommandLine()
    {
        _sourceOption = OptionsHelper.GetSourceOption();
        _destinationOption = OptionsHelper.GetDestinationOption();
        _offsetOption = OptionsHelper.GetOffsetOption();
    }
    
    public async Task<int> InitCommandLine(string[] args)
    {
        var rootCommand = new RootCommand("Rearrange files containing Exif data")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        AddDeleteEmptyDirectoryCommand(rootCommand);
        AddRearrangeByExifCommand(rootCommand);
        AddCheckIfFileNameContainsDateDifferentToExifDatesCommand(rootCommand);
        AddRearrangeByCameraManufacturerCommand(rootCommand);
        AddRearrangeBySoftwareCommand(rootCommand);
        AddFixExifDateByOffsetCommand(rootCommand);
        return await rootCommand.InvokeAsync(args);
    }

    private void AddDeleteEmptyDirectoryCommand(RootCommand rootCommand)
    {
        var deleteEmptyDirectoryCommand = new Command("deleteEmptyDirectory", "Search recursively for emtpy directories and delete them.")
        { 
            _sourceOption
        };
        deleteEmptyDirectoryCommand.SetHandler(DeleteEmptyDirectory!, _sourceOption);
        rootCommand.AddCommand(deleteEmptyDirectoryCommand);
    }

    private void AddRearrangeByExifCommand(RootCommand rootCommand)
    {
        var rearrangeByExifCommand = new Command("rearrangeByExif",
            "Scan source dir and move photos and videos to destination directory. Create subdirectory structure yyyy/MM/dd given by the Exif information of the image/video source. Xmp files are placed accordingly.")
        {
            _sourceOption,
            _destinationOption
        };
        rootCommand.AddCommand(rearrangeByExifCommand);
        rearrangeByExifCommand.SetHandler(SortImagesByExif!, _sourceOption, _destinationOption);
        rootCommand.AddCommand(rearrangeByExifCommand);
    }

    private void AddCheckIfFileNameContainsDateDifferentToExifDatesCommand(RootCommand rootCommand)
    {
        var checkIfFileNameContainsDateDifferentToExifDatesCommand = new Command(
            "checkIfFileNameContainsDateDifferentToExifDates",
            "check if image timestamp differs from exif and rename file")
        {
            _sourceOption
        };
        checkIfFileNameContainsDateDifferentToExifDatesCommand.SetHandler(
            CheckIfFileNameContainsDateDifferentToExifDates!, _sourceOption);
        rootCommand.AddCommand(checkIfFileNameContainsDateDifferentToExifDatesCommand);
    }

    private void AddRearrangeByCameraManufacturerCommand(RootCommand rootCommand)
    {
        var rearrangeByCameraManufacturerCommand = new Command("rearrangeByCameraManufacturer",
            "Find all images of certain camera. Sort into camera subdirectories. Keep layout but prepend camera manufacturer.")
        {
            _sourceOption,
            _destinationOption
        };
        rearrangeByCameraManufacturerCommand.SetHandler(SortImagesByManufacturer!, _sourceOption, _destinationOption);

        rootCommand.AddCommand(rearrangeByCameraManufacturerCommand);
    }

    private void AddRearrangeBySoftwareCommand(RootCommand rootCommand)
    {
        var rearrangeBySoftwareCommand = new Command("rearrangeBySoftware",
            "Find all F-Spot images. They might be wrong. Compare them. Keep layout but prepend software that was creating images.")
        {
            _sourceOption,
            _destinationOption
        };
        rearrangeBySoftwareCommand.SetHandler(RearrangeBySoftware!, _sourceOption, _destinationOption);

        rootCommand.AddCommand(rearrangeBySoftwareCommand);
    }

    private void AddFixExifDateByOffsetCommand(RootCommand rootCommand)
    {
        var fixExifDateByOffsetCommand = new Command(
            "fixExifDateByOffset",
            "Fix their exif by identifying the offset.")
        {
            _sourceOption,
            _offsetOption,
        };
        fixExifDateByOffsetCommand.SetHandler<DirectoryInfo, object>(FixExifDateByOffset!, _sourceOption!, _offsetOption!);

        rootCommand.AddCommand(fixExifDateByOffsetCommand);
    }

    private static void DeleteEmptyDirectory(DirectoryInfo directory)
    {
        Run(new DeleteEmptyDirectory(directory));
    }

    private static void SortImagesByExif(DirectoryInfo sourcePath, DirectoryInfo destinationPath)
    {
        Run(new SortImageByExif(sourcePath, destinationPath, _extensions));
    }

    private static void FixExifDateByOffset(DirectoryInfo directory, object offset)
    {
        // https://github.com/dotnet/command-line-api/issues/2086
        Run(new FixExifDateByOffset(directory, (TimeSpan) offset));
    }

    private static void RearrangeBySoftware(DirectoryInfo source, DirectoryInfo destination)
    {
        Run(new RearrangeBySoftware(source, destination));
    }

    private static void SortImagesByManufacturer(DirectoryInfo source, DirectoryInfo destination)
    { 
        Run(new SortImagesByManufacturer(source, destination));
    }

    private static void CheckIfFileNameContainsDateDifferentToExifDates(DirectoryInfo source)
    {  
        Run(new CheckIfFileNameContainsDateDifferentToExifDates(source));
    }
    
    private static void Run(IRun f)
    {
        try
        {
            Console.WriteLine(f.Run().PrintStatistics());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        };
    }
}