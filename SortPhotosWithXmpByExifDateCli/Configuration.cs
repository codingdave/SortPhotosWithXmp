using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;

namespace SortPhotosWithXmpByExifDateCli;

public static class Configuration
{
    private static LoggerConfiguration AddConsoleOrDebug(this LoggerSinkConfiguration loggerSinkConfiguration)
    {
        return Debugger.IsAttached ?
            loggerSinkConfiguration.Debug() :
            loggerSinkConfiguration.Console();
    }

    public static IHost CreateHost()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(GetBasePath())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Log.Logger = new LoggerConfiguration()
        //     .MinimumLevel.Verbose()
        //     .WriteTo.File(logFileName.FullName, /*rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10, */restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
        //     .WriteTo.AddConsoleOrDebug()
        //     .CreateLogger();

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
            // _ = Debugger.IsAttached ? loggingBuilder.AddDebug() : loggingBuilder.AddConsole();
            // _ = loggingBuilder.AddSerilog();
        })
        .UseSerilog()
        .Build();

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
