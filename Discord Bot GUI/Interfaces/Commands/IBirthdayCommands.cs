using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IBirthdayCommands
    {
        Task BirthdayAddForUser([Remainder] string inputParams);
        Task BirthdayRemoveForUser([Remainder] string userIdOrName);
        Task BirthdayAdd(string year, string month, string day);
        Task BirthdayRemove();
        Task BirthdayList();
    }
}
