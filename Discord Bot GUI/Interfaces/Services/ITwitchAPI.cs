using Discord_Bot.Services.Models.Twitch;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface ITwitchAPI
    {
        Task Start();
        UserData GetChannel(string username);
    }
}
