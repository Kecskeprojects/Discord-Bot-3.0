using Discord.Commands;
using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface IAudioService
    {
        Task RequestHandler(SocketCommandContext context, string input);
        Task PlayHandler(SocketCommandContext context, ServerResource server, ulong sId);
        Task<bool> ConnectBot(SocketCommandContext context, ServerResource server, ulong sId);
    }
}
