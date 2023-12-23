// dotnet ~/projects/SortPhotosWithXmp/SortPhotosWithXmp/bin/Release/net6.0/SortPhotosWithXmp.dll ~/Fotos ~/test
// Found 2670 images and 2699 xmps <-- 2670 images left
// trim trailing slash on directory parameters

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.CommandLine;
using SortPhotosWithXmp.Extensions;

using SystemInterface.IO;

namespace SortPhotosWithXmp;

public static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var host = Configuration.CreateHost();
        var logger = host.Services.GetRequiredService<ILogger<LoggerContext>>();
        var file = host.Services.GetRequiredService<IFile>();
        var directory = host.Services.GetRequiredService<IDirectory>();
        logger.TestInformationLevels();

        var commandLineHandler = new CommandLineHandler(logger, file, directory);
        return await commandLineHandler.InvokeAsync(args);
    }
}