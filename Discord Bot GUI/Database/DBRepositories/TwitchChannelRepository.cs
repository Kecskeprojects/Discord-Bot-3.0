using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class TwitchChannelRepository(MainDbContext context) : GenericRepository<TwitchChannel>(context), ITwitchChannelRepository
    {
    }
}
