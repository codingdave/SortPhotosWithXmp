// dotnet ~/projects/SortPhotosWithXmpByExifDate/SortPhotosWithXmpByExifDate.Cli/bin/Release/net6.0/SortPhotosWithXmpByExifDate.Cli.dll ~/Fotos ~/test
// Found 2670 images and 2699 xmps <-- 2670 images left
// trim trailing slash on directory parameters

namespace SortPhotosWithXmpByExifDate.Cli;

public static class Program
{
    static async Task<int> Main(string[] args)
    {
        var commandLine = new CommandLine();
        return await commandLine.InvokeAsync(args);
    }
}