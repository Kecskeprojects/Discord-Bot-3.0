using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class CustomCommandRepository(MainDbContext context) : BaseRepository(context), ICustomCommandRepository
    {
        public async Task AddCustomCommandAsync(CustomCommand command)
        {
            context.CustomCommands.Add(command);
            await context.SaveChangesAsync();
        }

        public Task<bool> CustomCommandExistsAsync(ulong serverId, string commandName)
        {
            return context.CustomCommands
                .Include(cc => cc.Server)
                .Where(cc => cc.Server.DiscordId == serverId.ToString() && cc.Command.Trim().ToLower().Equals(commandName.Trim().ToLower()))
                .AnyAsync();
        }

        public Task<CustomCommand> GetCustomCommandAsync(ulong serverId, string commandName)
        {
            return context.CustomCommands
                .Include(cc => cc.Server)
                .FirstOrDefaultAsync(cc => cc.Server.DiscordId == serverId.ToString() && cc.Command.Trim().ToLower().Equals(commandName.Trim().ToLower()));
        }

        public Task<List<CustomCommand>> GetCustomCommandListAsync(ulong serverId)
        {
            return context.CustomCommands
                .Include(cc => cc.Server)
                .Where(cc => cc.Server.DiscordId == serverId.ToString())
                .OrderBy(cc => cc.Command)
                .ToListAsync();
        }

        public async Task RemoveCustomCommandAsync(CustomCommand customCommand)
        {
            context.CustomCommands.Remove(customCommand);
            await context.SaveChangesAsync();
        }
    }
}
