using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IBirthdayCommands
    {
        Task BirthdayAddForUser(ulong userId, string year, string month = "", string day = "");
        Task BirthdayRemoveForUser(ulong userId);
        Task BirthdayAdd(string year, string month, string day);
        Task BirthdayRemove();
    }
}
