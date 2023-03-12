using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using DirectoryExtensions = MetadataExtractor.DirectoryExtensions;

namespace SortPhotosWithXmpByExifDateCli;

public static class Helpers
{
    public static string FixPath(string path)
    {
        if (path.Contains('~'))
        {
            path = path.Replace("~",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile,
                    Environment.SpecialFolderOption.DoNotVerify));
        }

        return path;
    }

    public static FileInfo[] GetCorrespondingXmpFiles(FileInfo fileInfo)
    {
        return Directory.GetFiles(
                Path.GetDirectoryName(fileInfo.FullName)
                ?? throw new InvalidOperationException(),
                $"{Path.GetFileNameWithoutExtension(fileInfo.FullName)}*.xmp", new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = false,
                })
            .Select(x => new FileInfo(x))
            .ToArray();
    }

    private static void PrintAllXmpData(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        foreach (var xmpDirectory in directories.OfType<XmpDirectory>())
        {
            if (xmpDirectory.XmpMeta == null) continue;
            foreach (var property in xmpDirectory.XmpMeta.Properties)
            {
                Console.WriteLine($"{property.Path}: {property.Value}");
            }
        }
    }

    private static void PrintAllData(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        foreach (var directory in directories)
        {
            foreach (var tag in directory.Tags)
            {
                // if (tag.Description != null && tag.Description.Contains("10:17"))
                {
                    Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
                }
            }
        }
    }

    public static void CheckForErrors(IReadOnlyList<MetadataExtractor.Directory> directories, FileInfo fileInfo)
    {
        foreach (var directory in directories)
        {
            if (!directory.HasError) continue;
            foreach (var error in directory.Errors)
            {
                // throw new InvalidOperationException($"ERROR: {fileInfo}: {error}");
                Console.Error.WriteLine($"{fileInfo}: {error}");
            }
        }
    }

    public static DateTime GetDateTimeFromImage(IReadOnlyList<MetadataExtractor.Directory> directories,
        FileInfo fileInfo)
    {
        // Exif IFD0 - Date/Time = 2023:01:18 10:54:28
        // Exif SubIFD - Date/Time Digitized = 2023:01:18 10:17:32
        // Exif SubIFD - Date/Time Original = 2023:01:18 10:17:32
        // IPTC - Date Created = 2023:01:18
        // IPTC - Digital Date Created = 2023:01:18
        // IPTC - Digital Time Created = 10:17:32+0100
        // IPTC - Time Created = 10:17:32+0100

        var exifId0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        if (exifId0Directory != null)
        {
            if (DirectoryExtensions.TryGetDateTime(exifId0Directory, ExifDirectoryBase.TagDateTime, out var tagDateTime))
            {
                return tagDateTime;
            }
        }

        var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (exifSubIfdDirectory != null)
        {
            if (DirectoryExtensions.TryGetDateTime(exifSubIfdDirectory, ExifDirectoryBase.TagDateTimeDigitized,
                    out var tagDateTimeDigitized))
            {
                return tagDateTimeDigitized;
            }
            else if (DirectoryExtensions.TryGetDateTime(exifSubIfdDirectory, ExifDirectoryBase.TagDateTimeOriginal,
                         out var tagDateTimeOriginal))
            {
                return tagDateTimeOriginal;
            }
        }

        var iptcDirectory = directories.OfType<IptcDirectory>().FirstOrDefault();
        if (iptcDirectory != null)
        {
            if (DirectoryExtensions.TryGetDateTime(iptcDirectory, IptcDirectory.TagDateCreated, out var tagDateCreated))
            {
                return tagDateCreated;
            }
            else if (DirectoryExtensions.TryGetDateTime(iptcDirectory, IptcDirectory.TagTimeCreated, out var tagTimeCreated))
            {
                return tagTimeCreated;
            }
            else if (DirectoryExtensions.TryGetDateTime(iptcDirectory, IptcDirectory.TagDigitalDateCreated, out var tagDigitalDateCreated))
            {
                return tagDigitalDateCreated;
            }
            else if (DirectoryExtensions.TryGetDateTime(iptcDirectory, IptcDirectory.TagDigitalTimeCreated, out var tagDigitalTimeCreated))
            {
                return tagDigitalTimeCreated;
            }
        }

        var quickTimeMovieHeaderDirectory = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
        if (quickTimeMovieHeaderDirectory != null)
        {
            if (DirectoryExtensions.TryGetDateTime(quickTimeMovieHeaderDirectory, ExifDirectoryBase.TagDateTimeOriginal,
                    out var tagDateTimeOriginal))
            {
                return tagDateTimeOriginal;
            }
            else if (DirectoryExtensions.TryGetDateTime(quickTimeMovieHeaderDirectory, QuickTimeMovieHeaderDirectory.TagCreated,
                         out var tagCreated))
            {
                return tagCreated;
            }
        }

        throw new InvalidOperationException();
    }

    public static void PrintMetadata(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        PrintAllData(directories);
        PrintAllXmpData(directories);
    }

    public static void MoveImageAndXmpToExifPath(FileInfo imageFile, FileInfo[] xmpFiles, DateTime dateTime,
        DirectoryInfo destinationDirectory, ImagesAndXmpFoundStatistics statistics, bool force)
    {
        var destinationSuffix = dateTime.ToString("yyyy/MM/dd");
        // /{Path.GetFileNameWithoutExtension(imageFile.Name)}
        // /{imageFile.Directory}
        var finalDestinationPath = $"{destinationDirectory.FullName}/{destinationSuffix}";
        if (!Directory.Exists(finalDestinationPath))
        {
            // Console.WriteLine($"Directory does not exist. Creating {finalDestinationPath}");
            Directory.CreateDirectory(finalDestinationPath);
        }

        statistics.FoundImages++;
        statistics.FoundXmps += xmpFiles.Count();

        var allSourceFiles = new List<FileInfo>() { imageFile };
        allSourceFiles.AddRange(xmpFiles);

        foreach (var f in allSourceFiles)
        {
            var targetName = $"{finalDestinationPath}/{f.Name}";
            // Console.WriteLine($"File.Move({f}, {targetName});");
            if (!File.Exists(targetName))
            {
                if(force)
                {
                    File.Move(f.FullName, targetName);
                }
            }
            else
            {
                Console.WriteLine($"Skipping existing {targetName}");
            }
        }

        // Console.WriteLine();
    }

    public static void RecursivelyDeleteEmptyDirectories(DirectoryInfo directory, DirectoriesDeletedStatistics statistics, bool force, bool start = true)
    {
        void DeleteDirectoryIfEmpty(DirectoryInfo d)
        {
            statistics.DirectoriesFound++;
            if(!d.GetDirectories().Any() && 
               !d.GetFiles().Any())
            {
                statistics.DirectoriesDeleted++;
                if(force)
                {
                    Directory.Delete(d.FullName, false);
                }
            }
        }

        foreach(var d in directory.GetDirectories())
        {
            RecursivelyDeleteEmptyDirectories(d, statistics, false);
            DeleteDirectoryIfEmpty(d);
        }

        if(start)
        {
            DeleteDirectoryIfEmpty(directory);
        }
    }
}