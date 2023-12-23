using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using SortPhotosWithXmp.Wrappers;

using SystemInterface.IO;

namespace SortPhotosWithXmp;

public static class Configuration
{
    private static string GetBasePath()
    {
        return Core.Configuration.GetBasePath();
    }

    public static IHost CreateHost()
    {
        var appsettings = GetBasePath() + "appsettings.json";
        var additionalAppsettings = GetBasePath() + $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json";

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(GetBasePath())
            .AddJsonFile(appsettings, optional: false, reloadOnChange: true)
            .AddJsonFile(additionalAppsettings, optional: true, reloadOnChange: true)
            ;

        var configurationRoot = configurationBuilder.Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configurationRoot)
            .Enrich.FromLogContext()
            .CreateLogger();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(serviceCollection =>
                _ = serviceCollection
                    // .AddTransient<IFile, FileWrap>()
                    .AddTransient<IFile, FileWrapper>()
                    // .AddTransient<IDirectory, DirectoryWrap>()
                    .AddTransient<IDirectory, DirectoryWrapper>()
                    )
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                _ = configurationBuilder
                    .SetBasePath(SortPhotosWithXmp.Core.Configuration.GetBasePath())
                    .AddJsonFile(appsettings, optional: false, reloadOnChange: true)
                    .AddJsonFile(path: additionalAppsettings, optional: true))
                    .ConfigureLogging(loggingBuilder =>
            {
            // _ = loggingBuilder
            //     .ClearProviders()
            //     .AddConfiguration();
            //     .AddSerilog()
            })
            .UseSerilog()
            .Build();

        Log.Information($"reading configuration from required '{appsettings}' and optional '{additionalAppsettings}'");
        return host;
    }
}