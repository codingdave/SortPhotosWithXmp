using System.Text.RegularExpressions;

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
    private static readonly string[] _extensions = new string[]
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

    private readonly string _sourceDirectory;

    public FileScanner(string sourceDirectory)
    {
        _sourceDirectory = sourceDirectory;
        GenerateDatabase();
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
            var allFilesWithExt = Directory.EnumerateFiles(_sourceDirectory, ext, enumerationOptions).AsParallel().ToArray();
            foreach (var file in allFilesWithExt)
            {
                if (notSupportedNamingRegex.IsMatch(file))
                {
                    throw new NotSupportedException($"The file '{file}' has an invalid name: Sidecar files will not be distiguishable from edits of another file. The convention to name them is: filename_number.extension.xmp, which matches this filename.");
                }

#warning use inheritance to use non-hash instances over here. Replace them with hash ones when necessary. Do not use null.
                files.Add(file, new FileVariations(new ImageFile(file), new List<IImageFile>()));
            }
        }

#warning check darktable to see how this is implemented
        Regex editRegex = new(@"(?<base>.*?)(_\d?\d?)?(?<extension>\.\w+)\" + SidecarFileExtension);
        var allSidecars = Directory.EnumerateFiles(_sourceDirectory, "*" + SidecarFileExtension, enumerationOptions);
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
            _all.Add(file.Value);
        }
    }

    public static string SidecarFileExtension { get; } = ".xmp";

    private readonly HashSet<FileVariations> _all = new();
    public IEnumerable<FileVariations> All => _all;

    public IEnumerable<FileVariations> MultipleEdits => All.Where(x => x.SidecarFiles.Count > 1);

    public IEnumerable<IImageFile> LonelySidecarFiles => All.Where(x => x.Data == null).SelectMany(x => x.SidecarFiles);
    public IEnumerable<FileVariations> HealtyFileVariations => All.Where(x => x.Data != null);
}
