using MetadataExtractor.Formats.Xmp;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;
using SortPhotosWithXmpByExifDateCli.Operation;
using SortPhotosWithXmpByExifDateCli.Repository;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli;

public static class Helpers
{
    public static string FixPath(string path)
    {
        if (path.Contains('~'))
        {
            path = path.Replace("~",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile,
                    Environment.SpecialFolderOption.DoNotVerify));
        }

        return path;
    }

    public static string[] GetCorrespondingXmpFiles(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            throw new ArgumentException($"'{nameof(file)}' cannot be null or whitespace.", nameof(file));
        }

        var directory = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("can not determine directory of file '{file}'");

        var searchPattern = Path.GetFileName(file) + "*" + FileScanner.SidecarFileExtension;
        var options = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = false,
        };
        return Directory.GetFiles(directory, searchPattern, options);
    }

    private static IList<string> GetPropertyDescriptions(XmpDirectory xmpDirectory)
    {
        List<string> propertyDescriptions = new();
        if (xmpDirectory.XmpMeta != null)
        {
            foreach (var property in xmpDirectory.XmpMeta.Properties)
            {
                if (property.Path != null && property.Value != null)
                {
                    propertyDescriptions.Add($"{property.Path}: {property.Value}");
                }
            }
        }

        return propertyDescriptions;
    }

    public static IEnumerable<string> GetErrorsFromMetadata(IReadOnlyList<MetadataExtractor.Directory> metaDataDirectories)
    {
        return metaDataDirectories.SelectMany(t => t.Errors);
    }

    public static List<string> GetMetadata(IReadOnlyList<MetadataExtractor.Directory> metaDataDirectories)
    {
        var ret = new List<string>();
        foreach (var metadataDirectory in metaDataDirectories)
        {
            foreach (var tag in metadataDirectory.Tags)
            {
                ret.Add($"{metadataDirectory.Name} - {tag.Name} = {tag.Description}");
            }
            if (metadataDirectory is XmpDirectory xmpDirectory)
            {
                ret.AddRange(GetPropertyDescriptions(xmpDirectory));
            }
        }

        return ret;
    }

    public static void MoveImageAndXmpToExifPath(string imageFile,
                                                 string[] xmpFiles,
                                                 DateTime dateTime,
                                                 string destinationDirectory,
                                                 FilesFoundStatistics statistics,
                                                 IFileOperation operationPerformer)
    {
        var destinationSuffix = dateTime.ToString("yyyy/MM/dd");
        var finalDestinationPath = Path.Combine(destinationDirectory, destinationSuffix);

        if (!Directory.Exists(finalDestinationPath))
        {
            Directory.CreateDirectory(finalDestinationPath);
        }

        statistics.FoundImages++;
        statistics.FoundXmps += xmpFiles.Length;

        var allSourceFiles = xmpFiles.ToList();
        allSourceFiles.Add(imageFile);

        foreach (var file in allSourceFiles)
        {
            var targetName = Path.Combine(finalDestinationPath, Path.GetFileName(file));

            if (!File.Exists(targetName))
            {
                operationPerformer.ChangeFile(file, targetName);
            }
            else
            {
                statistics.AddError(new FileAlreadyExistsError(targetName, file, $"File {file} already exists at {targetName}"));
            }
        }
    }

    public static void RecursivelyDeleteEmptyDirectories(ILogger logger, string directory, DeleteDirectoryOperation deleteDirectoryPerformer, bool isFirstRun = true)
    {
        void DeleteDirectoryIfEmpty(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    deleteDirectoryPerformer.Statistics.DirectoriesFound++;
                    // if no directories and no files are within this directory
                    if (!Directory.GetDirectories(directory).Any() && !Directory.GetFiles(directory).Any())
                    {
                        deleteDirectoryPerformer.DeleteDirectory(directory);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogExceptionError(e);
            }
        }

        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            RecursivelyDeleteEmptyDirectories(logger, subDirectory, deleteDirectoryPerformer);
            DeleteDirectoryIfEmpty(subDirectory);
        }

        if (isFirstRun)
        {
            DeleteDirectoryIfEmpty(directory);
        }
    }
}