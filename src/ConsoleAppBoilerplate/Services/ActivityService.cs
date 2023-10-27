using ConsoleAppBoilerplate.Services.Models;
using ConsoleAppBoilerplate.Upstream.Clients;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json;

namespace ConsoleAppBoilerplate.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ILogger<ActivityService> _logger;
        private readonly IBoredClient _boredClient;

        public ActivityService(ILogger<ActivityService> logger, IBoredClient boredClient)
        { 
            _logger = logger;
            _boredClient = boredClient;
        }

        public async Task<Activity?> GetActivity(int numberOfParticipants)
        {
            _logger.LogInformation($"Fetching activity for {numberOfParticipants}");

            var upstreamActivityModel = await _boredClient.GetActivity(numberOfParticipants);

            if(upstreamActivityModel == null)
            {
                return null;
            }

            _logger.LogInformation($"Received upstream model:\n{JsonSerializer.Serialize(upstreamActivityModel, new JsonSerializerOptions { WriteIndented = true })}");
            
            var activityModel = new Activity(upstreamActivityModel);

            _logger.LogInformation($"After conversion to service model:\n {JsonSerializer.Serialize(activityModel, new JsonSerializerOptions { WriteIndented = true })}");

            return activityModel;
        }
    }
}
