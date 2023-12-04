using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface ITwitterScraperCommands
    {
        Task ScrapeFromUrl([Remainder] string message);
    }
}
