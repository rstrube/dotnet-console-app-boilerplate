using System.Threading.Tasks;
using ConsoleAppBoilerplate.Upstream.Models;

namespace ConsoleAppBoilerplate.Upstream.Clients;

public interface IBoredClient
{
    public Task<BoredActivity?> GetActivity(int numberOfParticipants);
}