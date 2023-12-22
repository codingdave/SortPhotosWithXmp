// dotnet ~/projects/SortPhotosWithXmp/SortPhotosWithXmp/bin/Release/net6.0/SortPhotosWithXmp.dll ~/Fotos ~/test
// Found 2670 images and 2699 xmps <-- 2670 images left
// trim trailing slash on directory parameters

using SortPhotosWithXmp.CommandLine;

namespace SortPhotosWithXmp;

public static class Program
{
    static async Task<int> Main(string[] args)
    {
        var commandLineHandler = new CommandLineHandler();
        return await commandLineHandler.InvokeAsync(args);
    }
}