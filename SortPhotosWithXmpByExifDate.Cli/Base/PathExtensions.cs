namespace SortPhotosWithXmpByExifDate.Cli.Commands;

public static class PathExtensions
{
    public static string CreatePath(this string filePath)
    {
        var fixedPath = filePath.FixPath();
        var fullPath = Path.GetFullPath(fixedPath);
        var directory = new DirectoryWrapper();

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
