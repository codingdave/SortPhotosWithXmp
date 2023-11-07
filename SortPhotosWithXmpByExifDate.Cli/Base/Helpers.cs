using MetadataExtractor.Formats.Xmp;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Result;

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

   
}