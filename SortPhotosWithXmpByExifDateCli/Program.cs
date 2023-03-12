// dotnet ~/projects/SortPhotosWithXmpByExifDate/SortPhotosWithXmpByExifDateCli/bin/Release/net6.0/SortPhotosWithXmpByExifDateCli.dll ~/Fotos ~/test
// Found 2670 images and 2699 xmps <-- 2670 images left
// trim trailing slash on directory parameters

namespace SortPhotosWithXmpByExifDateCli;

public static class Program
{
    static async Task<int> Main(string[] args)
    {
        var commandLine = new CommandLine();
        return await commandLine.InitCommandLine(args);
    }
}