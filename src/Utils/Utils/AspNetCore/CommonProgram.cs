using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Serilog;

namespace Utils.AspNetCore;

public class CommonProgram
{
    public static readonly string AppName = Assembly.GetEntryAssembly().GetName().Name;
    public static string[] DefaultAdditionaConfigurationFiles = new[] { "appsettings.local.json" };

    /// <typeparam name="TStartup"></typeparam>
    /// <param name="args"></param>
    /// <param name="beforeRun"></param>
    /// <param name="additionalConfigurationFiles">additional appsetting.xxx.json files. Defaul CommonProgram.DefaultAdditionaConfigurationFiles</param>
    /// <returns></returns>
    public static int Main<TStartup>(string[] args,
        Action<IHost> beforeRun = null,
        string[] additionalConfigurationFiles = null
        ) where TStartup : class
    {
        var configuration = GetConfiguration(args, additionalConfigurationFiles);
        Log.Logger = CreateSerilogLogger(configuration);

        try
        {
            Log.Information("Configuring web host ({ApplicationContext})...", AppName);
            var hostBuilder = CreateHostBuilder<TStartup>(args, configuration);
            var host = hostBuilder.Build();

            beforeRun?.Invoke(host);

            Log.Information("Starting web host ({ApplicationContext})...", AppName);
            host.Run();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder<TStartup>(string[] args, string[] additionalConfigurationFiles = null)
        where TStartup : class
    {
        var configuration = GetConfiguration(args, additionalConfigurationFiles);
        return CreateHostBuilder<TStartup>(args, configuration);
    }

    private static IHostBuilder CreateHostBuilder<TStartup>(string[] args, IConfiguration configuration) where TStartup : class =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder
                .UseConfiguration(configuration)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddConfiguration(configuration);
                })
                .CaptureStartupErrors(false)
                .UseStartup<TStartup>()
                .UseSerilog()
                .UseKestrel();
            });

    /// <remarks>
    /// Similar as default configuration created by CreateDefaultBuilder
    /// https://github.com/dotnet/aspnetcore/blob/v5.0.11/src/DefaultBuilder/src/WebHost.cs#L169
    /// </remarks>
    private static IConfiguration GetConfiguration(string[] args, string[] additionalConfigurationFiles = null)
    {
        additionalConfigurationFiles ??= DefaultAdditionaConfigurationFiles;
        var environment = GetEnvironment();

        // Default
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables();

        if (args != null)
        {
            builder.AddCommandLine(args);
        }

        // Custom
        foreach (var file in additionalConfigurationFiles)
        {
            builder.AddJsonFile(file, optional: true);
        }

        return builder.Build();
    }

    private static string GetEnvironment()
    {
        // https://stackoverflow.com/a/37468237/11391667
        // https://github.com/dotnet/aspnetcore/blob/v5.0.11/src/Hosting/Hosting/src/WebHostBuilder.cs
        var w = new WebHostBuilder();
        return w.GetSetting(WebHostDefaults.EnvironmentKey);
    }

    private static ILogger CreateSerilogLogger(IConfiguration configuration)
    {
        var environment = GetEnvironment();

        var builder = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("ApplicationContext", AppName)
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.FromLogContext();

        builder = builder.WriteTo.Console();

        return builder.CreateLogger();
    }
}
