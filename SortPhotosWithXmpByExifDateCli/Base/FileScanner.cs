using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.Repository;

// Multiple edits:
// DSC_9287.NEF        <-- file, could be mov, jpg, ...
// DSC_9287.NEF.xmp    <-- 1.st development file
// DSC_9287_01.NEF.xmp <-- 2.nd development file 

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
            var allFilesWithExt = Directory.EnumerateFiles(scanDirectory, ext, enumerationOptions).AsParallel().ToArray();
            foreach (var file in allFilesWithExt)
            {
                if (ImageFileWithRevision.IsMatch(file))
                {
                    _logger.LogWarning($"The file '{file}' has an invalid name: Sidecar files will not be distiguishable from edits of another file. The convention to name them is: filename_number.extension.xmp, which matches this filename.");
                }

                files.Add(file, new FileVariations(new ImageFile(file), new List<IImageFile>()));
            }
        }

        // darktable appends .xmp to the filename. 
        // Thats it? How can it detect duplicates?
        // crawler.c:147 
        // #warning check darktable to see how this is implemented. Different xmp variations might be possible.
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

    public const string XmpExtension = ".xmp";
    
    private const string BaseNumber = @"(?<base>.*?)(?:_\d?\d?)";
    private const string Extension = @"(?<extension>\.\w+)";
    public Regex ImageFileWithRevision = new(BaseNumber + Extension);
    public Regex XmpFileWithOptionalRevision = new($"{BaseNumber}?{Extension}{XmpExtension}");

    public IEnumerable<FileVariations> All => _all;
    public IEnumerable<FileVariations> MultipleEdits => All.Where(x => x.SidecarFiles.Count > 1);
    public IEnumerable<IImageFile> LonelySidecarFiles => All.Where(x => x.Data == null).SelectMany(x => x.SidecarFiles);
    public IEnumerable<FileVariations> HealtyFileVariations => All.Where(x => x.Data != null);

    public string? ScanDirectory { get; private set; }
}
