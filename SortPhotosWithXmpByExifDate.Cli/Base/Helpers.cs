using MetadataExtractor.Formats.Xmp;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli;

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

    public static void MoveImageAndXmpToExifPath(
        IDirectory directory,
        FileVariations fileVariations,
        DateTime dateTime,
        string destinationDirectory,
        FilesFoundStatistics statistics,
        IFileOperation operationPerformer)
    {
        if (directory is null)
        {
            throw new ArgumentNullException(nameof(directory));
        }

        if (fileVariations.Data == null)
        {
            throw new InvalidOperationException($"The image that shall be moved was not found.");
        }

        if (string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException($"'{nameof(destinationDirectory)}' cannot be null or empty.", nameof(destinationDirectory));
        }

        if (statistics is null)
        {
            throw new ArgumentNullException(nameof(statistics));
        }

        if (operationPerformer is null)
        {
            throw new ArgumentNullException(nameof(operationPerformer));
        }

        var destinationSuffix = dateTime.ToString("yyyy/MM/dd");
        var finalDestinationPath = Path.Combine(destinationDirectory, destinationSuffix);

        if (!directory.Exists(finalDestinationPath))
        {
            _ = directory.CreateDirectory(finalDestinationPath);
        }

        statistics.FoundImages++;
        statistics.FoundXmps += fileVariations.SidecarFiles.Count;

        var allSourceFiles = fileVariations.SidecarFiles;
        allSourceFiles.Add(fileVariations.Data);

        foreach (var file in allSourceFiles)
        {
            var targetName = Path.Combine(finalDestinationPath, Path.GetFileName(file.OriginalFilename));

            if (!File.Exists(targetName))
            {
                operationPerformer.ChangeFile(file.OriginalFilename, targetName);
                file.NewFilename = targetName;
            }
            else
            {
                statistics.AddError(new FileAlreadyExistsError(targetName, file.OriginalFilename, $"File {file.OriginalFilename} already exists at {targetName}"));
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