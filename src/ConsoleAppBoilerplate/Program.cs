using System;
using ConsoleAppBoilerplate.Upstream.Clients;
using ConsoleAppBoilerplate.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using System.IO;
using ConsoleAppBoilerplate.Services;
using System.Threading.Tasks;

namespace ConsoleAppBoilerplate;

public sealed class Program
{   
    public static async Task<int> Main(string[] args)
    {
        // create a bootstrap logger, this will capture startup exceptions
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Default", LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var hostAppBuilder = CreateHostApplicationBuilder(args);

            // configure all services, including logging, IHostedService configured directly below
            ConfigureServices(hostAppBuilder);

            // explicitly add the IHostedService here
            hostAppBuilder.Services.AddHostedService<AppHostedService>();

            // build the app
            var app = hostAppBuilder.Build();

            // run the app
            await app.RunAsync();

            return Environment.ExitCode;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ConsoleAppBoilerplate terminated unexpectedly.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static HostApplicationBuilder CreateHostApplicationBuilder(string[] args)
    {
        // the Host.CreateApplicationBuilder() method will set many sane defaults for the HostApplicationBuilder
        var builder = Host.CreateApplicationBuilder(args);

        // including explicitly defined configuration here, even though many defaults are already set from method call above
        builder.Environment.ContentRootPath = Directory.GetCurrentDirectory();
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddCommandLine(args);

        return builder;
    }

    private static void ConfigureServices(HostApplicationBuilder builder)
    {
        // configure serilog via appSettings.json
        builder.Services.AddLogging(config =>
        {
            // clear all built in logging providers
            config.ClearProviders();

            // create a logger from the serilog configuration in appSettings.json
            Logger logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            // configure serilog as primary ILogger across the application
            config.AddSerilog(logger);
        });

        // get specific sections from appSettings.json
        var boredClientSection = builder.Configuration.GetSection(BoredClientConfig.Section);
        var activityParamsSection = builder.Configuration.GetSection(ActivityParamsConfig.Section);

        // configure IOptions<BoredClientConfig> to use the section above when passed in via DI
        builder.Services.Configure<BoredClientConfig>(boredClientSection);

        // configure IOptions<ActivityParamsConfig> to use teh section above when passed in via DI
        builder.Services.Configure<ActivityParamsConfig>(activityParamsSection);

        // create a local instance of BoredClientConfig that can be used during application startup
        var boredClientConfig = boredClientSection.Get<BoredClientConfig>();

        builder.Services.AddSingleton<IActivityService, ActivityService>();

        builder.Services.AddSingleton<MockBoredClient>();
        builder.Services.AddSingleton<BoredClient>();
        builder.Services.AddSingleton<IBoredClient>(sp =>
        {
            return boredClientConfig.UseMock
             ? sp.GetRequiredService<MockBoredClient>()
             : sp.GetRequiredService<BoredClient>();
        });

        // add additional services here:
        //builder.Services.AddTransient<IMyService, MyService>();
        //builder.Services.AddScoped<IMyService2, MyService2>();
    }
}