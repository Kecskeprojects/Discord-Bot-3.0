using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IOwnerCommands
    {
        public Task Help();
        public Task Say(IMessageChannel channel, [Remainder] string text);
    }
}
