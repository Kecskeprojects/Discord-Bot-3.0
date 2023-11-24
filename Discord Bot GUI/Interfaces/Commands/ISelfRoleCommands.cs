using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface ISelfRoleCommands
    {
        Task SelfRoleAdd([Remainder] string name);
        Task SelfRoleRemove([Remainder] string name);
        Task SendSelfRoleMessage();
    }
}
