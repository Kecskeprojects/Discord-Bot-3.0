using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IBiasAliasCommands
    {
        Task AddBiasAlias([Remainder] string biasData);
        Task RemoveBiasAlias([Remainder] string biasData);
    }
}
