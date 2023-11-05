using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Repository;

// Multiple edits:
// DSC_9287.NEF        <-- file, could be mov, jpg, ...
// DSC_9287.NEF.xmp    <-- 1.st development file, version 0. No versioning for first version.
// DSC_9287_01.NEF.xmp <-- 2.nd development file, version 1
// DSC_9287_02.NEF.xmp <-- 3.rd development file, version 2

// Multiple filetypes
// DSC_7708.JPG
// DSC_7708.JPG.xmp
// DSC_7708.NEF
// DSC_7708.NEF.xmp

public class FileScanner : IFileScanner
{
    private readonly string[] _extensions = new string[]
    {
        "*.jpg",
        "*.jpeg",
        "*.nef",
        "*.gif",
        "*.png",
        "*.psd",
        "*.cr3",
        "*.arw",
        "*.mp4",
        "*.mov",
    };

    private readonly HashSet<FileVariations> _all = new();
    private readonly ILogger _logger;

    public FileScanner(ILogger logger) => _logger = logger;

    public void Crawl(string sourceDirectory)
    {
        try
        {
            GenerateDatabase(sourceDirectory);
        }
        catch (Exception e)
        {
            _logger.LogExceptionError(e);
        }
    }


    private void GenerateDatabase(string scanDirectory)
    {
        ScanDirectory = scanDirectory;
        // we need to use the full filename without sidecar extension and without edit version as the key as the base for all variations:
        Dictionary<string, FileVariations> files = new();

        var enumerationOptions = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            MatchType = MatchType.Simple,
            RecurseSubdirectories = true
        };

        // does the filename look like an edit from another file?
        foreach (var ext in _extensions)
        {
            Directory
                .EnumerateFiles(scanDirectory, ext, enumerationOptions)
                .AsParallel()
                .ForAll(file => {
                        var filenameWithoutExtensionAndVersion = ExtractFilenameWithoutExtentionAndVersion(file);
                        files.Add(
                            filenameWithoutExtensionAndVersion,
                            new FileVariations(new ImageFile(file), new List<IImageFile>()));
                    }
                );
        }

        // darktable 
        // * appends .xmp to the filename. 
        // * image.c:498, (from behind) find the last point
        // * image.c:499, add version before with format "_%02d"
        // * image.c:501, (from behind) find the last point, append remaining characters (extension) to the end 
        var allSidecars = Directory.EnumerateFiles(scanDirectory, "*" + XmpExtension, enumerationOptions);
        foreach (var file in allSidecars)
        {
            var key = file;
            var match = XmpFileWithOptionalRevision.Match(key);
            if (match.Success)
            {
                // remove the .xmp extension
                // remove a possible _1 edit
                key = match.Groups["base"].Value + match.Groups["extension"].Value;
            }

            if (files.TryGetValue(key, out var value))
            {
                value.SidecarFiles.Add(new ImageFile(file));
            }
            else
            {
                value = new FileVariations(null, new List<IImageFile>() { new ImageFile(file) });
                files.Add(key, value);
            }
        }

        foreach (var file in files)
        {
#warning check if a collision is allowed or an error
            _ = _all.Add(file.Value);
        }
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
                    secondLastDot = p1;
                }
            }
            result = file[..secondLastDot];
        }
        else
        {
            // in case of images we just need to strip off the extension
            result = file[..lastDot];
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

    public IEnumerable<FileVariations> All => _all;
    public IEnumerable<FileVariations> MultipleEdits => All.Where(x => x.SidecarFiles.Count > 1);
    public IEnumerable<IImageFile> LonelySidecarFiles => All.Where(x => x.Data == null).SelectMany(x => x.SidecarFiles);
    public IEnumerable<FileVariations> HealtyFileVariations => All.Where(x => x.Data != null);

    public string? ScanDirectory { get; private set; }
}
