using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IChatCommands
    {
        Task Eightball([Remainder] string question);
        Task CustomList();
        Task Help();
        Task CoinFlip([Remainder] string choice = "");
        Task WotdFunction(string language = "korean");
    }
}
