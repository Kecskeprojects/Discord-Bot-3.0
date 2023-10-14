using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IBirthdayCommands
    {
        Task BirthdayAdd(string year, string month, string day);
        Task BirthdayRemove();
    }
}
