using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SystemInterface.IO;
namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public class FileScanner : IFileScanner
{
    private readonly HashSet<FileVariations> _all = new();
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
        var images = GetAllImagesInCurrentDirectory(directory);
        foreach (var image in images)
        {
            Map.Add(image, new(new ImageFile(image), new()));
        }

        var xmps = GetAllXmpsInCurrentDirectory(directory);
        foreach (var file in xmps)
        {
            var filenameWithoutExtensionAndVersion = ExtractFilenameWithoutExtentionAndVersion(file);

            if (Map.TryGetValue(filenameWithoutExtensionAndVersion, out var value))
            {
                value.SidecarFiles.Add(new ImageFile(file));
            }
            else
            {
                _logger.LogDebug($"Expected base image {filenameWithoutExtensionAndVersion} not found for {file}");
                value = new FileVariations(null, new List<IImageFile>() { new ImageFile(file) });
                Map.Add(filenameWithoutExtensionAndVersion, value);
            }
        }

        foreach (var file in Map)
        {
            if (!_all.Add(file.Value))
            {
                throw new InvalidOperationException("key already present. This should not be possible.");
            }
        }
    }

    public IEnumerable<string> GetAllXmpsInCurrentDirectory(IDirectory directory)
    {
        var regexString = @".*\" + XmpExtension + "$";
        var extensionRegex = new Regex(regexString, RegexOptions.IgnoreCase);
        var path = directory.GetCurrentDirectory();

        return FindMatchingFiles(directory, extensionRegex, path);
    }

    public IEnumerable<string> GetAllImagesInCurrentDirectory(IDirectory directory)
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

        var regexString = @".*\.(?:" + string.Join(@"|", extensions) + ")$";
        var extensionRegex = new Regex(regexString, RegexOptions.IgnoreCase);
        var path = directory.GetCurrentDirectory();

        return FindMatchingFiles(directory, extensionRegex, path);
    }

    private static IEnumerable<string> FindMatchingFiles(IDirectory directory, Regex extensionRegex, string path)
    {
        #warning Check AsParallel and measure
        var res = directory
            .EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Where(x => extensionRegex.IsMatch(x))
            .ToList();

        return res;
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

    public IEnumerable<FileVariations> All => _all;
    public IEnumerable<FileVariations> MultipleEdits => All.Where(x => x.SidecarFiles.Count > 1);
    public IEnumerable<IImageFile> LonelySidecarFiles => All.Where(x => x.Data == null).SelectMany(x => x.SidecarFiles);
    public IEnumerable<FileVariations> HealtyFileVariations => All.Where(x => x.Data != null);

    public string? ScanDirectory { get; private set; }

    public IDictionary<string, FileVariations> Map { get; }
}