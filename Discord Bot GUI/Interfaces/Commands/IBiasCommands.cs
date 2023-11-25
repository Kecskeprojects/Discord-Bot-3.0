using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IBiasCommands
    {
        public Task AddBiasList([Remainder] string biasName);
        public Task RemoveBiasList([Remainder] string biasName);
        public Task AddBias([Remainder] string biasName);
        public Task RemoveBias([Remainder] string biasName);
        public Task ClearBias();
        public Task MyBiases([Remainder] string groupName = "");
        public Task BiasList([Remainder] string groupName = "");
        public Task PingBias([Remainder] string bias);
    }
}
