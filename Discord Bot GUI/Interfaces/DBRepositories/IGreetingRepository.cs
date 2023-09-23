using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IGreetingRepository
    {
        Task AddGreetingAsync(Greeting greeting);
        Task<List<Greeting>> GetAllGreetingAsync();
        Task<Greeting> GetGreetingByIdAsync(int id);
        Task RemoveGreetingAsync(Greeting greeting);
    }
}
