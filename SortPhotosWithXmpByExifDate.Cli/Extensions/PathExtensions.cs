using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Extensions;

public static class PathExtensions
{
    public static string CreatePath(this string filePath, IDirectory directory)
    {
        var fixedPath = filePath.FixPath();
        var fullPath = Path.GetFullPath(fixedPath);

        if (!directory.Exists(fullPath))
        {
            _ = directory.CreateDirectory(fullPath);
        }
        return filePath;
    }

    public static string FixPath(this string path)
    {
        if (path.Contains('~'))
        {
            path = path.Replace("~",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile,
                    Environment.SpecialFolderOption.DoNotVerify));
        }

        return path;
    }

}
