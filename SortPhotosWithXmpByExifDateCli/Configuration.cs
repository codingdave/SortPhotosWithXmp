using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli;

public static class Configuration
{
    public static IHost CreateHost()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Minute, retainedFileCountLimit: 10)
            .CreateLogger();

        var host = Host.CreateDefaultBuilder()
        .ConfigureServices(serviceCollection =>
        {
        })
        .ConfigureAppConfiguration(configurationBuilder =>
        {
            _ = configurationBuilder
                .SetBasePath(GetBasePath())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureLogging(loggingBuilder =>
        {
            _ = loggingBuilder.ClearProviders();
            _ = Debugger.IsAttached ? loggingBuilder.AddDebug() : loggingBuilder.AddConsole();
            _= loggingBuilder.AddSerilog();
        })
        .Build();
        return host;
    }

    private static string GetBasePath()
    {
        // using var processModule = Process.GetCurrentProcess().MainModule;
        // return Path.GetDirectoryName(processModule?.FileName) ?? string.Empty;
        // return Directory.GetCurrentDirectory();
        // return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        return System.AppContext.BaseDirectory;
    }
}
