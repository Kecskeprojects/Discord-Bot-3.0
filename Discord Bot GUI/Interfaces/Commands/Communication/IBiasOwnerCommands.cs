using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands.Communication
{
    public interface IBiasOwnerCommands
    {
        public Task AddBiasList([Remainder] string biasName);
        public Task RemoveBiasList([Remainder] string biasName);
    }
}
