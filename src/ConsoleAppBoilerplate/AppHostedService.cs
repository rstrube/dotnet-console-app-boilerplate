using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using ConsoleAppBoilerplate.Services;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ConsoleAppBoilerplate.Configuration;

namespace ConsoleAppBoilerplate;

public class AppHostedService : IHostedService
{
    private int? _exitCode;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<AppHostedService> _logger;
    private readonly IOptions<ActivityParamsConfig> _activityParamConfig;
    private readonly IActivityService _activityService;

    public AppHostedService(
                IHostApplicationLifetime hostApplicationLifetime,   
                ILogger<AppHostedService> logger,
                IOptions<ActivityParamsConfig> activityParamConfig,
                IActivityService activityService)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _activityParamConfig = activityParamConfig;
        _activityService = activityService;

        _hostApplicationLifetime.ApplicationStarted.Register(OnStartup);
        _hostApplicationLifetime.ApplicationStopped.Register(OnShutdown);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting hosted service...");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down hosted service...");
        Environment.ExitCode = _exitCode ?? 1;
        return Task.CompletedTask;
    }

    public async void OnStartup()
    {
        _logger.LogInformation("Startup for hosted service has completed.");

        try
        {
            var random = new Random();

            // grab min and max participants from configuartion (appSettings.json) via IOptions
            var configMinNumberOfParticipants = _activityParamConfig.Value.MinNumberOfParticipants;
            var configMaxNumberOfParticipants = _activityParamConfig.Value.MaxNumberOfParticipants;

            _logger.LogInformation($"Configured minimum number of participants: {configMinNumberOfParticipants}");
            _logger.LogInformation($"Configured maximum number of participants: {configMaxNumberOfParticipants}");

            // generate a random number of participants from the configured min / max
            var numberOfParticipants = random.Next(configMinNumberOfParticipants, configMaxNumberOfParticipants);

            Console.WriteLine($"Retrieving activity suggestion for {numberOfParticipants} participant(s)...");

            var activity = await _activityService.GetActivity(numberOfParticipants);

            if (activity == null)
            {
                _logger.LogError($"Unable to retrieve activity for {numberOfParticipants} participant(s).");
                Console.WriteLine($"Error: Unable to retrieve activity for {numberOfParticipants} participant(s).");

                _exitCode = 1;
                return;

            }
            Console.WriteLine($"Suggested activity:");
            Console.WriteLine(JsonSerializer.Serialize(activity, new JsonSerializerOptions() { WriteIndented = true }));
            
            _exitCode = 0;
            return;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in hosted service.");
        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }

    private void OnShutdown()
    {
        _logger.LogInformation("Shutdown for hosted service has completed.");
    }
 }
