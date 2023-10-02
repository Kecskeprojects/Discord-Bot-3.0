using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IChatCommands
    {
        Task Help();
        Task Eightball([Remainder] string question);
        Task CoinFlip([Remainder] string choice = "");
        Task WotdFunction(string language = "korean");
    }
}
