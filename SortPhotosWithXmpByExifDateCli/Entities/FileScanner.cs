using System.IO.Enumeration;
using System.Text.RegularExpressions;

namespace SortPhotosWithXmpByExifDateCli.Entities;

public class FileScanner
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

    private static readonly string _xmpExtension = ".xmp";

    private readonly string _sourceDirectory;

    public FileScanner(string sourceDirectory)
    {
        _sourceDirectory = sourceDirectory;

        // we need to use the full filename without sidecar extension and without edit version as the key as the base for all variations:
        Dictionary<string, FileVariations> files = new();

        var enumerationOptions = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            MatchType = MatchType.Simple,
            RecurseSubdirectories = true
        };

        foreach (var ext in _extensions)
        {
            var allFilesWithExt = Directory.EnumerateFiles(_sourceDirectory, ext, enumerationOptions).AsParallel().ToArray();
            foreach (var file in allFilesWithExt)
            {
                files.Add(file, new FileVariations(file, new List<string>()));
            }
        }

        Regex editRegex = new Regex(@"(?<base>.*?)(_\d?\d?)?(?<extension>\.\w+)\.xmp");
        var allSidecars = Directory.EnumerateFiles(_sourceDirectory, "*" + _xmpExtension, enumerationOptions);
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
                value.SidecarFiles.Add(file);
            }
            else
            {
                value = new FileVariations(null, new List<string>() { file });
                files.Add(key, value);
            }
        }

        foreach (var file in files)
        {
            All.Add(file.Value);
        }
    }

    public HashSet<FileVariations> All { get; } = new();

    public IEnumerable<FileVariations> MultipleEdits => All.Where(x => x.SidecarFiles.Count > 1);

    public IEnumerable<string> LonelySidecarFiles => All.Where(x => x.Filename == null).SelectMany(x => x.SidecarFiles);
}

// Multiple edits:
// DSC_9287.NEF        <-- file, could be mov, jpg, ...
// DSC_9287.NEF.xmp    <-- 1.st development file
// DSC_9287_01.NEF.xmp <-- 2.nd development file 

// Multiple filetypes
// DSC_7708.JPG
// DSC_7708.JPG.xmp
// DSC_7708.NEF
// DSC_7708.NEF.xmp

#warning First Moments
// Problem
// ./images/11223344/Foo_First_Moments_11.JPG.xmp
// ./images/11223344/Foo_First_Moments_13.JPG.xmp
// ./images/11223344/Foo_First_Moments_4.JPG
// ./images/11223344/Foo_First_Moments_15.JPG.xmp
// ./images/11223344/Foo_First_Moments_10.JPG.xmp
// ./images/11223344/Foo_First_Moments_6.JPG
// ./images/11223344/Foo_First_Moments_8.JPG
// ./images/11223344/Foo_First_Moments_16.JPG.xmp
// ./images/11223344/Foo_First_Moments_14.JPG
// ./images/11223344/Foo_First_Moments_16.JPG
// ./images/11223344/Foo_First_Moments_12.JPG
// ./images/11223344/Foo_First_Moments_9.JPG
// ./images/11223344/Foo_First_Moments_7.JPG
// ./images/11223344/Foo_First_Moments_1.JPG.xmp
// ./images/11223344/Foo_First_Moments_13.JPG
// ./images/11223344/Foo_First_Moments_6.JPG.xmp
// ./images/11223344/Foo_First_Moments_2.JPG
// ./images/11223344/Foo_First_Moments_10.JPG
// ./images/11223344/Foo_First_Moments_1.JPG
// ./images/11223344/Foo_First_Moments_5.JPG
// ./images/11223344/Foo_First_Moments_7.JPG.xmp
// ./images/11223344/Foo_First_Moments_12.JPG.xmp
// ./images/11223344/Foo_First_Moments_3.JPG
// ./images/11223344/Foo_First_Moments_14.JPG.xmp
// ./images/11223344/Foo_First_Moments_5.JPG.xmp
// ./images/11223344/Foo_First_Moments_15.JPG
// ./images/11223344/Foo_First_Moments_4.JPG.xmp
// ./images/11223344/Foo_First_Moments_11.JPG
// ./images/11223344/Foo_First_Moments_8.JPG.xmp
// ./images/11223344/Foo_First_Moments_3.JPG.xmp
// ./images/11223344/Foo_First_Moments_2.JPG.xmp
// ./images/11223344/Foo_First_Moments_9.JPG.xmp
public record struct FileVariations(string? Filename, List<string> SidecarFiles);