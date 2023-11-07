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