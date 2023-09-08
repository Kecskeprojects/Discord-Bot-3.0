using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IReminderCommands
    {
        Task RemindAt([Remainder] string message);
        Task RemindIn([Remainder] string message);
        Task RemindList();
        Task RemindRemove(int reminderId);
    }
}
