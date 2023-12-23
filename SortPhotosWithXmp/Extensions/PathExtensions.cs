using SystemInterface.IO;

namespace SortPhotosWithXmp.Extensions;

public static class PathExtensions
{
    public static string CreatePath(this string filePath, IDirectory directoryWrapper)
    {
        var fixedPath = filePath.FixPath();
        var fullPath = Path.GetFullPath(fixedPath);

        if (!directoryWrapper.Exists(fullPath))
        {
            _ = directoryWrapper.CreateDirectory(fullPath);
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
