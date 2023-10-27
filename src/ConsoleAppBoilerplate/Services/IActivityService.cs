using ConsoleAppBoilerplate.Services.Models;
using System.Threading.Tasks;

namespace ConsoleAppBoilerplate.Services
{
    public interface IActivityService
    {
        public Task<Activity?> GetActivity(int numberOfParticipants);
    }
}
