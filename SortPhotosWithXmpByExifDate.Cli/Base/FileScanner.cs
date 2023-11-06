using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SortPhotosWithXmpByExifDate.Cli.Extensions;

using SystemInterface.IO;
namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public class FileScanner : IFileScanner
{
    private readonly ILogger _logger;

    public FileScanner(ILogger logger)
    {
        _logger = logger;
        Map = new Dictionary<string, FileVariations>();
    }

    public void Crawl(IDirectory directory)
    {
        try
        {
            GenerateDatabase(directory);
        }
        catch (Exception e)
        {
            _logger.LogExceptionError(e);
        }
    }


    private void GenerateDatabase(IDirectory directory)
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
                    // We did not find the image, so we assume it does not exist
                    _logger.LogDebug($"Expected base image {filenameWithoutExtensionAndVersion} not found for {file}");
                    value = new FileVariations(null, new List<IImageFile>() { new ImageFile(file) });
                    Map.Add(filenameWithoutExtensionAndVersion, value);
                }
            }
        });
    }

    public (IEnumerable<string> images, IEnumerable<string> xmps) GetAllImageDataInCurrentDirectory(IDirectory directory)
    {
        var extensions = new string[]
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

        var xmpRegexString = @".*\" + XmpExtension + "$";
        var xmpRegex = new Regex(xmpRegexString, RegexOptions.IgnoreCase);

        var imageRegexString = @".*\.(?:" + string.Join(@"|", extensions) + ")$";
        var imageRegex = new Regex(imageRegexString, RegexOptions.IgnoreCase);
        var path = directory.GetCurrentDirectory();

        return FindMatchingFiles(directory, imageRegex, xmpRegex, path);
    }

    private static (IEnumerable<string> images, IEnumerable<string> xmps) FindMatchingFiles(IDirectory directory, Regex imageRegex, Regex xmpRegex, string path)
    {
#warning Check AsParallel and measure
        var files = directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
#if DEBUG
            .ToList()
#endif
            ;

        var images = files.Where(x => imageRegex.IsMatch(x))
#if DEBUG
            .ToList()
#endif
            ;

        var xmps = files.Where(x => xmpRegex.IsMatch(x))
#if DEBUG
            .ToList()
#endif
            ;

        return (images, xmps);
    }


    public string ExtractFilenameWithoutExtentionAndVersion(string file)
    {
        // DSC_9287.NEF        <-- file, could be mov, jpg, ...
        // DSC_9287.NEF.xmp    <-- 1.st development file, version 0. No versioning for first version.
        // DSC_9287_01.NEF.xmp <-- 2.nd development file, version 1
        // DSC_9287_02.NEF.xmp <-- 3.rd development file, version 2

        var lastDot = file.LastIndexOf('.');
        string result;
        if (string.Equals(file[lastDot..], XmpExtension, StringComparison.OrdinalIgnoreCase))
        {
            // strip off image/second extension from filename.jpg.xmp 
            var secondLastDot = file.LastIndexOf('.', lastDot - 1);
            if (secondLastDot == -1)
            {
                // there is no second dot. The filename is like DSC_0051.xmp.
                // So we keep the filename as is
                result = file;
            }
            else
            {
                var endOfFilenameWithoutVersion = secondLastDot;
                // when we have a sidecar file, we might have a versioned one. 
                // invalid ending: _00, as this suffix will only exist for verions > 0
                // valid ending: _[0-9][0-9]
                var p1 = secondLastDot - 3;
                var p2 = secondLastDot - 2;
                var p3 = secondLastDot - 1;
                if (file[p1] == '_')
                {
                    if (file[p2] == '0' && file[p3] == '0')
                    {
                        // do nothing
                    }
                    else if (IsDigit(file[p2]) && IsDigit(file[p3]))
                    {
                        endOfFilenameWithoutVersion = p1;
                    }
                }
                result = string.Concat(file[..endOfFilenameWithoutVersion], file[secondLastDot..lastDot]);
            }
        }
        else
        {
            // in case of images we keep it as it is
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
