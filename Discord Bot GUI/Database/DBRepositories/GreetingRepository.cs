using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class GreetingRepository : BaseRepository, IGreetingRepository
    {
        public GreetingRepository(MainDbContext context) : base(context)
        {
        }

        public async Task AddGreetingAsync(Greeting greeting)
        {
            context.Greetings.Add(greeting);
            await context.SaveChangesAsync();
        }

        public Task<List<Greeting>> GetAllGreetingAsync() =>
            context.Greetings.ToListAsync();

        public Task<Greeting> GetGreetingByIdAsync(int id) => context.Greetings.SingleOrDefaultAsync(g => g.GreetingId == id);

        public async Task RemoveGreetingAsync(Greeting greeting)
        {
            context.Greetings.Remove(greeting);
            await context.SaveChangesAsync();
        }
    }
}
