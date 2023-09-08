using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class CustomCommandRepository : BaseRepository, ICustomCommandRepository
    {
        public CustomCommandRepository(MainDbContext context) : base(context)
        {
        }

        public Task<CustomCommand> GetCustomCommandAsync(ulong serverId, string commandName)
        {
            return context.CustomCommands
                .Include(cc => cc.Server)
                .FirstOrDefaultAsync(cc => cc.Server.DiscordId == serverId.ToString() && cc.Command.Trim().ToLower() == commandName.Trim().ToLower());
        }

        public Task<List<CustomCommand>> GetCustomCommandAsync(ulong serverId)
        {
            return context.CustomCommands
                .Include(cc => cc.Server)
                .Where(cc => cc.Server.DiscordId == serverId.ToString())
                .ToListAsync();
        }
    }
}
