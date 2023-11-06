using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SortPhotosWithXmpByExifDate.Cli.Extensions;

using SystemInterface.IO;
namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public class FileScanner : IFileScanner
{
    private readonly string[] _extensions = new string[]
    {
        "jpg",
        "jpeg",
        "nef",
        "gif",
        "png",
        "psd",
        "cr3",
        "arw",
        "mp4",
        "mov"
    };

    private readonly ILogger _logger;

    public FileScanner(ILogger logger)
    {
        _logger = logger;
        Map = new Dictionary<string, FileVariations>();
    }

    public void Crawl(IDirectory directory)
    {
        ScanDirectory = directory.GetCurrentDirectory();

        // Multiple filetypes make the extension mandatory and
        // Multiple edits share the same originating file/key DSC_9287.NEF:
        //
        // DSC_7708.JPG        <-- originating file/key
        // DSC_7708.JPG.xmp    <-- 1.st development file, version 0. No versioning for first version.
        // DSC_7708.NEF        <-- originating file/key
        // DSC_7708.NEF.xmp    <-- 1.st development file, version 0. No versioning for first version.
        // DSC_7708_01.NEF.xmp <-- 2.nd development file, version 1
        // DSC_7708_02.NEF.xmp <-- 3.rd development file, version 2
        // -> we need to use the full filename 
        // without sidecar extension and without edit version 
        // as the key as the base for all variations.

        // find all images
        var (images, xmps) = GetAllImageDataInCurrentDirectory(directory);

        images.ForEach(image => Map.Add(image, new(new ImageFile(image), new())));
        xmps.ForEach(file =>
        {
            var filenameWithoutExtensionAndVersion = ExtractFilenameWithoutExtentionAndVersion(file);

            if (Map.TryGetValue(filenameWithoutExtensionAndVersion, out var value))
            {
                value.SidecarFiles.Add(new ImageFile(file));
            }
            else
            {
                // Could be a wrong positive:                 
                // some/other/path/050826_foo_03.JPG.xmp could be Version0 for some/other/path/050826_foo_03.JPG but detected as Version3
                if (
                    string.Equals(file[^XmpExtension.Length..], XmpExtension, StringComparison.OrdinalIgnoreCase)
                    && Map.TryGetValue(file[..^XmpExtension.Length], out var fileVariation))
                {
                    fileVariation.SidecarFiles.Add(new ImageFile(file));
                }
                else
                {
                    if (!string.Equals(filenameWithoutExtensionAndVersion, file))
                    {
                        // We did not find the image, so we assume it does not exist
                        _logger.LogDebug($"Expected base image '{filenameWithoutExtensionAndVersion}' not found for '{file}'");
                    }
                    value = new FileVariations(null, new List<IImageFile>() { new ImageFile(file) });
                    Map.Add(filenameWithoutExtensionAndVersion, value);
                }
            }
        });
    }

    public (IList<string> images, IList<string> xmps) GetAllImageDataInCurrentDirectory(IDirectory directory)
    {
        var xmpRegexString = @".*\" + XmpExtension + "$";
        var xmpRegex = new Regex(xmpRegexString, RegexOptions.IgnoreCase);

        var imageRegexString = @".*\.(?:" + string.Join(@"|", _extensions) + ")$";
        var imageRegex = new Regex(imageRegexString, RegexOptions.IgnoreCase);
        var path = directory.GetCurrentDirectory();

        return FindMatchingFiles(directory, imageRegex, xmpRegex, path);
    }

    private (IList<string> images, IList<string> xmps)
        FindMatchingFiles(IDirectory directory, Regex imageRegex, Regex xmpRegex, string path)
    {
        _logger.LogInformation($"Scanning '{path}' for images and sidecar files.");
        var sw = new Stopwatch();
        sw.Start();

        var files = directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).AsParallel();
        var images = files.Where(x => imageRegex.IsMatch(x)).ToList();
        var xmps = files.Where(x => xmpRegex.IsMatch(x)).ToList();

        sw.Stop();
        _logger.LogInformation($"Found {images.Count} images and {xmps.Count} sidecar files in {sw.ElapsedMilliseconds / 1000}s.");

        return (images, xmps);
    }


    public string ExtractFilenameWithoutExtentionAndVersion(string file)
    {
        // DSC_9287.NEF        <-- file, could be mov, jpg, ...
        // DSC_9287.NEF.xmp    <-- 1.st development file, version 0. No versioning for first version.
        // DSC_9287_01.NEF.xmp <-- 2.nd development file, version 1
        // DSC_9287_02.NEF.xmp <-- 3.rd development file, version 2

        var lastDot = file.LastIndexOf('.');
        var secondLastDot = file.LastIndexOf('.', lastDot - 1);
        var p1 = secondLastDot - 3;
        var p2 = secondLastDot - 2;
        var p3 = secondLastDot - 1;
        string result;
        if (
            // does the file have 2 dots like in $nameWithPossibleVersion.$ImageExtension.xmp?
            secondLastDot != -1
            // does it end with .xmp?
            && string.Equals(file.Substring(lastDot, XmpExtension.Length), XmpExtension, StringComparison.OrdinalIgnoreCase)
            // is between the 2 dots a known $ImageExtension?
            && _extensions.Any(x => string.Equals(x, file.Substring(secondLastDot + 1, lastDot - secondLastDot - 1), StringComparison.OrdinalIgnoreCase)))
        {
            // move before $ImageExtension in filename.$ImageExtension.xmp 
            var endOfFilenameWithoutVersion = secondLastDot;
            // when we have a sidecar file, we might have a versioned one. 
            // Then we will find _00 - _99 in the filename:
            // * _00: invalid ending: as version 0 will not have that suffix attached
            // * _[0-9][0-9]: valid ending, suffix exists for all versions from 01-99

            if (file[p1] == '_')
            {
                if (file[p2] == '0' && file[p3] == '0')
                {
                    // _00 is invalid - do nothing
                }
                else if (IsDigit(file[p2]) && IsDigit(file[p3]))
                {
                    // _01-99: strip off _xx
                    endOfFilenameWithoutVersion = p1;
                }
            }
            // assemble $nameWithoutVersion.$ImageExtension
            result = string.Concat(file[..endOfFilenameWithoutVersion], file[secondLastDot..lastDot]);
        }
        else
        {
            // we keep the filename if 
            // * there is no second dot (like for DSC_0051.xmp)
            // * it does not end in .xmp (like for DSC0051.01.xmp)
            // * between the 2 dots is no known extension
            result = file;
        }

        return result;
    }

    private bool IsDigit(char c)
    {
        return c is '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9';
    }

    public const string XmpExtension = ".xmp";

    private const string BaseNumber = @"(?<base>.*?)(?:_\d?\d?)";
    private const string Extension = @"(?<extension>\.\w+)";
    public Regex XmpFileWithOptionalRevision = new($"{BaseNumber}?{Extension}{XmpExtension}");

    public IEnumerable<FileVariations> MultipleEdits => Map.Values.Where(x => x.SidecarFiles.Count > 1);
    public IEnumerable<IImageFile> LonelySidecarFiles => Map.Values.Where(x => x.Data == null).SelectMany(x => x.SidecarFiles);
    public IEnumerable<FileVariations> HealtyFileVariations => Map.Values.Where(x => x.Data != null);

    public string? ScanDirectory { get; private set; }

    public IDictionary<string, FileVariations> Map { get; }
}
