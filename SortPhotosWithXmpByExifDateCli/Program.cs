// dotnet ~/projects/SortPhotosWithXmpByExifDate/SortPhotosWithXmpByExifDateCli/bin/Release/net6.0/SortPhotosWithXmpByExifDateCli.dll ~/Fotos ~/test
// Found 2670 images and 2699 xmps <-- 2670 images left
// trim trailing slash on directory parameters

using System.CommandLine;
using MetadataExtractor;
using Directory = System.IO.Directory;

namespace SortPhotosWithXmpByExifDateCli;

public static class Program
{
    private static readonly Statistics Statistics = new();

    static async Task<int> Main(string[] args)
    {
        return await ParseCommandLine(args);
    }

    private static async Task<int> ParseCommandLine(string[] args)
    {
        var sourceOption = GetSourceOption();
        var destinationOption = GetDestinationOption();

        var rearrangeByExifCommand = new Command("rearrangeByExif",
            "Scan source dir and move photos and videos to destination directory in subdirectories given by the Exif information. Xmp files are placed accordingly.")
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
            sourceOption
        };
        fixExifDateByOffsetCommand.SetHandler(FixExifDateByOffset!, sourceOption);

        var rootCommand = new RootCommand("Rearrange files containing Exif data");
        rootCommand.AddCommand(rearrangeByExifCommand);
        rootCommand.AddCommand(checkIfFileNameContainsDateDifferentToExifDatesCommand);
        rootCommand.AddCommand(rearrangeByCameraManufacturerCommand);
        rootCommand.AddCommand(rearrangeBySoftwareCommand);
        rootCommand.AddCommand(fixExifDateByOffsetCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static void FixExifDateByOffset(FileInfo dir)
    {
        throw new NotImplementedException();
    }

    private static void RearrangeBySoftware(FileInfo source, FileInfo destination)
    {
        throw new NotImplementedException();
    }

    private static void SortImagesByManufacturer(FileInfo source, FileInfo destination)
    {
        throw new NotImplementedException();
    }

    private static void CheckIfFileNameContainsDateDifferentToExifDates(FileInfo source)
    {
        throw new NotImplementedException();
    }

    private static Option<FileInfo?> GetDestinationOption()
    {
        return new Option<FileInfo?>(
            name: "--destination",
            description: "The destination directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                    return null;
                }

                filePath = Helpers.FixPath(filePath);

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                return new FileInfo(filePath);
            }
        );
    }

    private static Option<FileInfo?> GetSourceOption()
    {
        return new Option<FileInfo?>(
            name: "--source",
            description: "The source directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                    return null;
                }

                filePath = Helpers.FixPath(filePath);

                if (!Directory.Exists(filePath))
                {
                    result.ErrorMessage = "Source directory does not exist";
                    return null;
                }
                else
                {
                    return new FileInfo(filePath);
                }
            }
        );
    }

    private static void SortImagesByExif(FileInfo searchPath, FileInfo destinationPath)
    {
        Console.WriteLine($"called {nameof(SortImagesByExif)}");
        Console.WriteLine(
            $"Starting {nameof(SortPhotosWithXmpByExifDateCli)}.{nameof(SortImagesByExif)} with search path: '{searchPath}' and destination path '{destinationPath}'");

        var searchDirectory = searchPath.Directory ?? throw new ArgumentNullException(nameof(searchPath.Directory));
        var files = searchDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(s =>
                s.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                s.Name.EndsWith(".nef", StringComparison.OrdinalIgnoreCase) ||
                s.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                s.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                s.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                s.Name.EndsWith(".cr3", StringComparison.OrdinalIgnoreCase));

        foreach (var fileInfo in files)
        {
            Statistics.FoundImages++;
            Console.WriteLine($"Found photo {fileInfo}");
            IReadOnlyList<MetadataExtractor.Directory>
                directories = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
            try
            {
                Helpers.CheckForErrors(directories, fileInfo);
                var dateTime = Helpers.GetDateTimeFromImage(directories, fileInfo);

                var xmpFiles = Helpers.GetCorrespondingXmpFiles(fileInfo);
                if (xmpFiles.Length > 0)
                {
                    foreach (var xmpFile in xmpFiles)
                    {
                        Console.WriteLine($"found xmp {xmpFile} for {fileInfo}");
                        Statistics.FoundXmps++;
                    }
                }

                Helpers.MoveImageAndXmpToExifPath(fileInfo, xmpFiles, dateTime, destinationPath);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{fileInfo}: {e.Message}, {e}");
                Helpers.PrintMetadata(directories);
            }
        }

        Console.WriteLine(Statistics);
    }
}