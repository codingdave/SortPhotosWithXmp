using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SortPhotosWithXmpByExifDateCli;

public static class Configuration
{
    public static IHost CreateHost()
    {
        var appsettings = GetBasePath() + "appsettings.json";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(GetBasePath())
            .AddJsonFile(appsettings, optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var host = Host.CreateDefaultBuilder()
        .ConfigureServices(serviceCollection => { })
        .ConfigureAppConfiguration(configurationBuilder =>
        {
            _ = configurationBuilder
                .SetBasePath(GetBasePath())
                .AddJsonFile(appsettings, optional: false, reloadOnChange: true);
        })
        .ConfigureLogging(loggingBuilder =>
        {
            _ = loggingBuilder.ClearProviders();
        })
        .UseSerilog()
        .Build();

        Log.Information($"reading configuration from '{appsettings}'");
        return host;
    }

    public static string GetBasePath()
    {
        // using var processModule = Process.GetCurrentProcess().MainModule;
        // return Path.GetDirectoryName(processModule?.FileName) ?? string.Empty;
        // return Directory.GetCurrentDirectory();
        // return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        return AppContext.BaseDirectory;
    }
}
