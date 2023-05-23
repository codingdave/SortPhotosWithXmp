using MetadataExtractor.Formats.Xmp;
using Microsoft.Extensions.Logging;
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

    public static FileInfo[] GetCorrespondingXmpFiles(FileInfo fileInfo)
    {
        if (fileInfo?.Directory is null) { throw new ArgumentNullException(nameof(fileInfo)); }

        var filename = fileInfo.Name + "*.xmp";
        var options = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = false,
        };
        return fileInfo.Directory.GetFiles(filename, options);
    }

    private static List<string> GetAllXmpData(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var ret = new List<string>();
        foreach (var xmpDirectory in directories.OfType<XmpDirectory>())
        {
            if (xmpDirectory.XmpMeta != null)
            {
                foreach (var property in xmpDirectory.XmpMeta.Properties)
                {
                    if (property.Path != null && property.Value != null)
                    {
                        ret.Add($"{property.Path}: {property.Value}");
                    }
                }
            }
        }

        return ret;
    }

    private static List<string> GetAllData(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var ret = new List<string>();
        foreach (var directory in directories)
        {
            foreach (var tag in directory.Tags)
            {
                ret.Add($"{directory.Name} - {tag.Name} = {tag.Description}");
            }
        }

        return ret;
    }

    public static IError GetError(FileInfo fileInfo, IReadOnlyList<MetadataExtractor.Directory> metaDataDirectories)
    {
        return GetError(fileInfo, metaDataDirectories.SelectMany(t => t.Errors));
    }

    public static IError GetError(FileInfo fileInfo, IEnumerable<string> errors)
    {
        IError ret = errors.Count() switch
        {
            0 => new NoError(fileInfo),
            1 => new SingleError(fileInfo, errors.First()),
            _ => new MultipleErrors(fileInfo, errors),
        };
        return ret;
    }


    public static List<string> GetMetadata(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var ret = GetAllData(directories);
        ret.AddRange(GetAllXmpData(directories));
        return ret;
    }

    public static void MoveImageAndXmpToExifPath(ILogger logger, FileInfo imageFile, FileInfo[] xmpFiles, DateTime dateTime,
        DirectoryInfo destinationDirectory, ImagesAndXmpFoundStatistics statistics, bool force, bool move)
    {
        var destinationSuffix = dateTime.ToString("yyyy/MM/dd");
        var finalDestinationPath = Path.Combine(destinationDirectory.FullName, destinationSuffix);
        if (!Directory.Exists(finalDestinationPath))
        {
            Directory.CreateDirectory(finalDestinationPath);
        }

        statistics.FoundImages++;
        statistics.FoundXmps += xmpFiles.Length;

        var allSourceFiles = xmpFiles.ToList();
        allSourceFiles.Add(imageFile);

        foreach (var f in allSourceFiles)
        {
            var targetName = Path.Combine(finalDestinationPath, f.Name); ;

            if (!File.Exists(targetName))
            {
                if (force)
                {
                    if (move)
                    {
                        File.Move(f.FullName, targetName);
                    }
                    else
                    {
                        File.Copy(f.FullName, targetName);
                    }
                }
                else
                {
                    if (move)
                    {
                        logger.LogInformation($"File.Move({f.FullName}, {targetName});");
                    }
                    else
                    {
                        logger.LogInformation($"File.Copy({f.FullName}, {targetName});");
                    }
                }
            }
            else
            {
                statistics.AddError(new SingleError(f, $"File already exists at {targetName}"));
            }
        }
    }

    public static void RecursivelyDeleteEmptyDirectories(DirectoryInfo directory, DirectoriesDeletedStatistics statistics, bool force, bool isFirstRun = true)
    {
        void DeleteDirectoryIfEmpty(DirectoryInfo d)
        {
            statistics.DirectoriesFound++;
            if (!d.GetDirectories().Any() &&
               !d.GetFiles().Any())
            {
                statistics.DirectoriesDeleted++;
                if (force)
                {
                    Directory.Delete(d.FullName, false);
                }
            }
        }

        foreach (var d in directory.GetDirectories())
        {
            RecursivelyDeleteEmptyDirectories(d, statistics, false);
            DeleteDirectoryIfEmpty(d);
        }

        if (isFirstRun)
        {
            DeleteDirectoryIfEmpty(directory);
        }
    }
}