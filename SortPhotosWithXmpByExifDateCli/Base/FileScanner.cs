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

    internal FileScanner(ILogger<CommandLine> logger, string sourceDirectory)
    {
        _logger = logger;
        ScanDirectory = sourceDirectory;
        try
        {
            GenerateDatabase();
        }
        catch (Exception e)
        {
            _logger.LogExceptionError(e);
        }
    }

    private void GenerateDatabase()
    {
        // we need to use the full filename without sidecar extension and without edit version as the key as the base for all variations:
        Dictionary<string, FileVariations> files = new();

        var enumerationOptions = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            MatchType = MatchType.Simple,
            RecurseSubdirectories = true
        };

        // does the filename look like an edit from another file?
        Regex notSupportedNamingRegex = new(@"(?:.*?)(_\d?\d?)(?:\.\w+)");
        foreach (var ext in _extensions)
        {
            var allFilesWithExt = Directory.EnumerateFiles(ScanDirectory, ext, enumerationOptions).AsParallel().ToArray();
            foreach (var file in allFilesWithExt)
            {
                if (notSupportedNamingRegex.IsMatch(file))
                {
                    throw new NotSupportedException($"The file '{file}' has an invalid name: Sidecar files will not be distiguishable from edits of another file. The convention to name them is: filename_number.extension.xmp, which matches this filename.");
                }

                files.Add(file, new FileVariations(new ImageFile(file), new List<IImageFile>()));
            }
        }

#warning check darktable to see how this is implemented. Different xmp variations might be possible.
        Regex editRegex = new(@"(?<base>.*?)(_\d?\d?)?(?<extension>\.\w+)\" + SidecarFileExtension);
        var allSidecars = System.IO.Directory.EnumerateFiles(ScanDirectory, "*" + SidecarFileExtension, enumerationOptions);
        foreach (var file in allSidecars)
        {
            var key = file;
            var match = editRegex.Match(key);
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

    public static string SidecarFileExtension { get; } = ".xmp";

    private readonly HashSet<FileVariations> _all = new();
    private readonly ILogger<CommandLine> _logger;

    public IEnumerable<FileVariations> All => _all;

    public IEnumerable<FileVariations> MultipleEdits => All.Where(x => x.SidecarFiles.Count > 1);

    public IEnumerable<IImageFile> LonelySidecarFiles => All.Where(x => x.Data == null).SelectMany(x => x.SidecarFiles);
    public IEnumerable<FileVariations> HealtyFileVariations => All.Where(x => x.Data != null);

    public string ScanDirectory { get; }
}
