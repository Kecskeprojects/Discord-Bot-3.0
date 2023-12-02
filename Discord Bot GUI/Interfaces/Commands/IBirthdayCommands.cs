using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IBirthdayCommands
    {
        Task BirthdayAddForUser(string userIdOrName, string year, string month = "", string day = "");
        Task BirthdayRemoveForUser(string userIdOrName);
        Task BirthdayAdd(string year, string month, string day);
        Task BirthdayRemove();
        Task BirthdayList();
    }
}
