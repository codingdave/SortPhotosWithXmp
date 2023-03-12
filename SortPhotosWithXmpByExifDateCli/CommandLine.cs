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

    private static Option<object?> GetOffsetOption()
    {
        // To workaround the following issue we return an object instead of a struct 
        // "resource": "/home/david/projects/SortPhotosWithXmpByExifDate/SortPhotosWithXmpByExifDateCli/CommandLine.cs",
        // "message": "Argument 4: cannot convert from 'System.CommandLine.Option<System.TimeSpan?>' to 'System.CommandLine.Binding.IValueDescriptor<System.TimeSpan>' [SortPhotosWithXmpByExifDateCli]",
        // "startLineNumber": 148,
        return new Option<object?>(
            name: "--offset",
            description: "The offset that should be added to the images.",
            isDefault: true,
            parseArgument: result =>
            {
                TimeSpan? ret = null;
                var offset = result.Tokens.SingleOrDefault()?.Value;
                if (offset == null)
                {
                    result.ErrorMessage = "No argument given";
                } 
                else if (TimeSpan.TryParse(offset, out var parsed))
                {
                    ret = parsed;
                }
                else 
                {
                    result.ErrorMessage = $"cannot parse TimeSpan '{offset}'";
                }

                return ret;
            }
        );
    }
    
    private static Option<DirectoryInfo?> GetDestinationOption()
    {
        return new Option<DirectoryInfo?>(
            name: "--destination",
            description: "The destination directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                DirectoryInfo? ret = null;
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                } 
                else 
                {
                    filePath = Helpers.FixPath(filePath);

                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    ret = new DirectoryInfo(filePath);
                }
               
                return ret;
            }
        );
    }

    private static Option<DirectoryInfo?> GetSourceOption()
    {
        return new Option<DirectoryInfo?>(
            name: "--source",
            description: "The source directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                DirectoryInfo? ret = null;
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                } 
                else 
                {
                    filePath = Helpers.FixPath(filePath);

                    if (!Directory.Exists(filePath))
                    {
                        result.ErrorMessage = "Source directory does not exist";
                    }
                    else
                    {
                        ret = new DirectoryInfo(filePath);
                    }
                }

                return ret;
            }
        );
    }
    
    public static async Task<int> InitCommandLine(string[] args)
    {
        ImagesAndXmpFoundStatistics statistics = new();
        
        var sourceOption = GetSourceOption();
        var destinationOption = GetDestinationOption();
        var offsetOption = GetOffsetOption();

        var deleteEmptyDirectoryCommand = new Command("deleteEmptyDirectory", "Search recursively for emtpy directories and delete them.")
        { 
            sourceOption
        };
        deleteEmptyDirectoryCommand.SetHandler(DeleteEmptyDirectory!, sourceOption);

        var rearrangeByExifCommand = new Command("rearrangeByExif",
            "Scan source dir and move photos and videos to destination directory. Create subdirectory structure yyyy/MM/dd given by the Exif information of the image/video source. Xmp files are placed accordingly.")
        {
            sourceOption,
            destinationOption
        };
        rearrangeByExifCommand.SetHandler(SortImagesByExif!, sourceOption, destinationOption);

        var checkIfFileNameContainsDateDifferentToExifDatesCommand = new Command(
            "checkIfFileNameContainsDateDifferentToExifDates",
            "check if image timestamp differs from exif and rename file")
        {
            sourceOption
        };
        checkIfFileNameContainsDateDifferentToExifDatesCommand.SetHandler(
            CheckIfFileNameContainsDateDifferentToExifDates!, sourceOption);

        var rearrangeByCameraManufacturerCommand = new Command("rearrangeByCameraManufacturer",
            "Find all images of certain camera. Sort into camera subdirectories. Keep layout but prepend camera manufacturer.")
        {
            sourceOption,
            destinationOption
        };
        rearrangeByCameraManufacturerCommand.SetHandler(SortImagesByManufacturer!, sourceOption, destinationOption);

        var rearrangeBySoftwareCommand = new Command("rearrangeBySoftware",
            "Find all F-Spot images. They might be wrong. Compare them. Keep layout but prepend software that was creating images.")
        {
            sourceOption,
            destinationOption
        };
        rearrangeBySoftwareCommand.SetHandler(RearrangeBySoftware!, sourceOption, destinationOption);

        var fixExifDateByOffsetCommand = new Command(
            "fixExifDateByOffset",
            "Fix their exif by identifying the offset.")
        {
            sourceOption,
            offsetOption,
        };
        fixExifDateByOffsetCommand.SetHandler<DirectoryInfo, object>(FixExifDateByOffset!, sourceOption!, offsetOption!);

        var rootCommand = new RootCommand("Rearrange files containing Exif data")
        {
            TreatUnmatchedTokensAsErrors = true
        };
        rootCommand.AddCommand(deleteEmptyDirectoryCommand);
        rootCommand.AddCommand(rearrangeByExifCommand);
        rootCommand.AddCommand(checkIfFileNameContainsDateDifferentToExifDatesCommand);
        rootCommand.AddCommand(rearrangeByCameraManufacturerCommand);
        rootCommand.AddCommand(rearrangeBySoftwareCommand);
        rootCommand.AddCommand(fixExifDateByOffsetCommand);

        return await rootCommand.InvokeAsync(args);
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