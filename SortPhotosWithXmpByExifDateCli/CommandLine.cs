using System.CommandLine;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

internal class CommandLine
{
    private static readonly IEnumerable<string> _extensions = new List<string>()
    {
        ".jpg",
        ".nef",
        ".gif",
        ".mp4",
        ".png",
        ".cr3"
    };

    private readonly Option<DirectoryInfo?> _sourceOption;
    private readonly Option<DirectoryInfo?> _destinationOption;
    private readonly Option<object?> _offsetOption;

    private readonly Option<bool> _forceOption;

    private readonly RootCommand _rootCommand;

    public CommandLine()
    {
        _sourceOption = OptionsHelper.GetSourceOption();
        _destinationOption = OptionsHelper.GetDestinationOption();
        _offsetOption = OptionsHelper.GetOffsetOption();
        _forceOption = OptionsHelper.GetForceOption();

        _rootCommand = new RootCommand("Rearrange files containing Exif data")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        AddDeleteEmptyDirectoryCommand();
        AddRearrangeByExifCommand();

        // TODO: Add command to print files whose filenames contain a timestamp and for which the xmp time information differs. Also allow to specify the format for the time, like yyyy/MM/dd
        // TODO Allow to specify the format for the time, like yyyy/MM/dd
        // AddCheckIfFileNameContainsDateDifferentToExifDatesCommand();
        AddRearrangeByCameraManufacturerCommand();
        AddRearrangeBySoftwareCommand();
        AddFixExifDateByOffsetCommand();
    }

    public async Task<int> InvokeAsync(string[] args)
    {
        return await _rootCommand.InvokeAsync(args);
    }

    private void AddDeleteEmptyDirectoryCommand()
    {
        static void DeleteEmptyDirectory(DirectoryInfo directory, bool force)
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
        static void SortImagesByExif(DirectoryInfo sourcePath, DirectoryInfo destinationPath, bool force)
        {
            Run(new SortImageByExif(sourcePath, destinationPath, _extensions, force));
        }

        var rearrangeByExifCommand = new Command("rearrangeByExif",
            "Scan source dir and move photos and videos to destination directory. Create subdirectory structure yyyy/MM/dd given by the Exif information of the image/video source. Xmp files are placed accordingly.")
        {
            _sourceOption,
            _destinationOption
        };

        rearrangeByExifCommand.SetHandler(SortImagesByExif!, _sourceOption, _destinationOption, _forceOption);

        _rootCommand.AddCommand(rearrangeByExifCommand);
    }

    private void AddCheckIfFileNameContainsDateDifferentToExifDatesCommand()
    {
        static void CheckIfFileNameContainsDateDifferentToExifDates(DirectoryInfo source)
        {
            Run(new CheckIfFileNameContainsDateDifferentToExifDates(source));
        }

        var checkIfFileNameContainsDateDifferentToExifDatesCommand = new Command(
            "checkIfFileNameContainsDateDifferentToExifDates",
            "There might be situations in which the filename contains the correct timecheck if image timestamp differs from exif and rename file.")
        {
            _sourceOption
        };

        checkIfFileNameContainsDateDifferentToExifDatesCommand.SetHandler(
            CheckIfFileNameContainsDateDifferentToExifDates!, _sourceOption);

        _rootCommand.AddCommand(checkIfFileNameContainsDateDifferentToExifDatesCommand);
    }

    private void AddRearrangeByCameraManufacturerCommand()
    {
        static void SortImagesByManufacturer(DirectoryInfo source, DirectoryInfo destination, bool force)
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
        static void RearrangeBySoftware(DirectoryInfo source, DirectoryInfo destination, bool force)
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
        static void FixExifDateByOffset(DirectoryInfo directory, object offset, bool force)
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

    private static void Run(IRun f)
    {
        try
        {
            var statstics = f.Run();

            var errorDirectory = new DirectoryInfo("ErrorFiles");
            Console.WriteLine($"Copy files to {errorDirectory.FullName}");
            if (!errorDirectory.Exists)
            {
                errorDirectory.Create();
            }

            statstics.ReadOnlyFileError.CopyErrorFiles(errorDirectory);

            Console.WriteLine(statstics.PrintStatistics());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        };
    }
}