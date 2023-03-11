using MetadataExtractor;

namespace SortPhotosWithXmpByExifDateCli;

internal class SortImageByExif : IRun
{
    private readonly DirectoryInfo _destinationDirectory;
    private readonly DirectoryInfo _sourceDirectory;
    private readonly IEnumerable<string> _extensions;
    private readonly Statistics _statistics = new Statistics();

    internal SortImageByExif(DirectoryInfo sourceDirectoryInfo, DirectoryInfo destinationDirectoryInfo, IEnumerable<string> extensions)
    {
        _sourceDirectory = sourceDirectoryInfo ?? throw new ArgumentNullException(nameof(sourceDirectoryInfo));
        _destinationDirectory = destinationDirectoryInfo ?? throw new ArgumentNullException(nameof(destinationDirectoryInfo));
        _extensions = extensions;
    }

    public Statistics Run()
    {
        Console.WriteLine($"called {nameof(SortImageByExif)}::{nameof(Run)}");
        Console.WriteLine(
            $"Starting {nameof(SortPhotosWithXmpByExifDateCli)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'");

        var files = _sourceDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(f => _extensions.Any(e => f.Name.EndsWith(e, StringComparison.OrdinalIgnoreCase)));
        // s.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        // s.Name.EndsWith(".nef", StringComparison.OrdinalIgnoreCase) ||
        // s.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
        // s.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
        // s.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        // s.Name.EndsWith(".cr3", StringComparison.OrdinalIgnoreCase));

        foreach (var fileInfo in files)
        {
            _statistics.FoundImages++;
            Console.WriteLine($"Found photo {fileInfo}");
            IReadOnlyList<MetadataExtractor.Directory>
                directories = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
            try
            {
                Helpers.CheckForErrors(directories, fileInfo);
                var dateTime = Helpers.GetDateTimeFromImage(directories, fileInfo);

                var xmpFiles = Helpers.GetCorrespondingXmpFiles(fileInfo);
                if (xmpFiles.Length > 0)
                {
                    foreach (var xmpFile in xmpFiles)
                    {
                        Console.WriteLine($"found xmp {xmpFile} for {fileInfo}");
                        _statistics.FoundXmps++;
                    }
                }

                Helpers.MoveImageAndXmpToExifPath(fileInfo, xmpFiles, dateTime, _destinationDirectory);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{fileInfo}: {e.Message}, {e}");
                Helpers.PrintMetadata(directories);
            }
        }

        return _statistics;
    }
}