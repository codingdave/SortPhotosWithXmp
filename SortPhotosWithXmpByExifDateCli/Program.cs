// See https://aka.ms/new-console-template for more information

// check if image timestamp differs from exif and rename file
// Find all images of certain camera. Fix their exif by identifying the offset.
// Find all fspot images. They might be wrong. Compare them.
// Sort into camera subdirectories.
// dotnet ~/projects/SortPhotosWithXmpByExifDate/SortPhotosWithXmpByExifDateCli/bin/Release/net6.0/SortPhotosWithXmpByExifDateCli.dll ~/Fotos ~/test
// Found 2670 images and 2699 xmps <-- 2670 images left

using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using Directory = System.IO.Directory;

var images = 0;
var xmps = 0;

Console.WriteLine($"Parameters: {string.Join(", ", args)}");
switch (args.Length)
{
    case < 1:
        throw new InvalidOperationException("no search path was set");
    case < 2:
        throw new InvalidOperationException("no destination path was set");
}

string FixPath(string path)
{
    if (path.Contains('~'))
    {
        path = path.Replace("~",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile,
                Environment.SpecialFolderOption.DoNotVerify));
    }

    return path;
}

var searchPath = FixPath(args[0]);
var destinationPath = FixPath(args[1]);

Console.WriteLine(
    $"Starting SortPhotosWithXmpByExifDateCli with search path: '{searchPath}' and destination path '{destinationPath}'");

var enumerationOptions = new EnumerationOptions
{
    MatchCasing = MatchCasing.CaseInsensitive,
    RecurseSubdirectories = true,
};

FileInfo[] GetCorrespondingXmpFiles(FileInfo fileInfo)
{
    var localEnumerationOptions = new EnumerationOptions
    {
        MatchCasing = MatchCasing.CaseInsensitive,
        RecurseSubdirectories = false,
    };

    return Directory.GetFiles(
            Path.GetDirectoryName(fileInfo.FullName)
            ?? throw new InvalidOperationException(),
            $"{Path.GetFileNameWithoutExtension(fileInfo.FullName)}*.xmp", localEnumerationOptions)
        .Select(x => new FileInfo(x))
        .ToArray();
}

void PrintAllXmpData(IReadOnlyList<MetadataExtractor.Directory> directories, FileInfo fileInfo1)
{
    foreach (var directory in directories)
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
}

void PrintAllData(IReadOnlyList<MetadataExtractor.Directory> directories, FileInfo fileInfo1)
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

void CheckForErrors(IReadOnlyList<MetadataExtractor.Directory> directories, FileInfo fileInfo)
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

DateTime GetDateTimeFromImage(IReadOnlyList<MetadataExtractor.Directory> directories, FileInfo fileInfo)
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
        if (exifId0Directory.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var tagDateTime))
        {
            return tagDateTime;
        }
    }

    var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
    if (exifSubIfdDirectory != null)
    {
        if (exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var tagDateTimeDigitized))
        {
            return tagDateTimeDigitized;
        }
        else if (exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var tagDateTimeOriginal))
        {
            return tagDateTimeOriginal;
        }
    }

    var iptcDirectory = directories.OfType<IptcDirectory>().FirstOrDefault();
    if (iptcDirectory != null)
    {
        if (iptcDirectory.TryGetDateTime(IptcDirectory.TagDateCreated, out var tagDateCreated))
        {
            return tagDateCreated;
        }
        else if (iptcDirectory.TryGetDateTime(IptcDirectory.TagTimeCreated, out var tagTimeCreated))
        {
            return tagTimeCreated;
        }
        else if (iptcDirectory.TryGetDateTime(IptcDirectory.TagDigitalDateCreated, out var tagDigitalDateCreated))
        {
            return tagDigitalDateCreated;
        }
        else if (iptcDirectory.TryGetDateTime(IptcDirectory.TagDigitalTimeCreated, out var tagDigitalTimeCreated))
        {
            return tagDigitalTimeCreated;
        }
    }

    var quickTimeMovieHeaderDirectory = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
    if (quickTimeMovieHeaderDirectory != null)
    {
        if (quickTimeMovieHeaderDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal,
                out var tagDateTimeOriginal))
        {
            return tagDateTimeOriginal;
        }
        else if (quickTimeMovieHeaderDirectory.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated,
                     out var tagCreated))
        {
            return tagCreated;
        }
    }

    throw new InvalidOperationException();
}

void PrintMetadata(IReadOnlyList<MetadataExtractor.Directory> directories, FileInfo fileInfo)
{
    PrintAllData(directories, fileInfo);
    PrintAllXmpData(directories, fileInfo);
}

var files = Directory.EnumerateFiles(searchPath, "*.*", SearchOption.AllDirectories)
    .Where(s =>
        s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        s.EndsWith(".nef", StringComparison.OrdinalIgnoreCase) ||
        s.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
        s.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
        s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        s.EndsWith(".cr3", StringComparison.OrdinalIgnoreCase));

foreach (var file in files)
{
    images++;
    var fileInfo = new FileInfo(file);
    Console.WriteLine($"Found photo {fileInfo}");
    IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
    try
    {
        CheckForErrors(directories, fileInfo);
        var dateTime = GetDateTimeFromImage(directories, fileInfo);

        var xmpFiles = GetCorrespondingXmpFiles(fileInfo);
        if (xmpFiles.Length > 0)
        {
            foreach (var xmpFile in xmpFiles)
            {
                Console.WriteLine($"found xmp {xmpFile} for {fileInfo}");
                xmps++;
            }
        }

        MoveImageAndXmpToExifPath(fileInfo, xmpFiles, dateTime);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine($"{fileInfo}: {e.Message}, {e}");
        PrintMetadata(directories, fileInfo);
    }
}

void MoveImageAndXmpToExifPath(FileInfo imageFile, FileInfo[] xmlFiles, DateTime dateTime)
{
    var destinationSuffix = dateTime.ToString("yyyy/MM/dd");
    // /{Path.GetFileNameWithoutExtension(imageFile.Name)}
    // /{imageFile.Directory}
    var finalDestinationPath = $"{destinationPath}/{destinationSuffix}";
    if (!Directory.Exists(finalDestinationPath))
    {
        Console.WriteLine($"Directory does not exist. Creating {finalDestinationPath}");
        Directory.CreateDirectory(finalDestinationPath);
    }

    var allSourceFiles = new List<FileInfo>() { imageFile };
    allSourceFiles.AddRange(xmlFiles);

    foreach (var f in allSourceFiles)
    {
        var targetName = $"{finalDestinationPath}/{f.Name}";
        // Console.WriteLine($"File.Move({f}, {targetName});");
        if (!File.Exists(targetName))
        {
            File.Move(f.FullName, targetName);
        }
        else
        {
            Console.WriteLine($"Skipping existing {targetName}");
        }
    }

    Console.WriteLine();
}

Console.WriteLine($"Found {images} images and {xmps} xmps");