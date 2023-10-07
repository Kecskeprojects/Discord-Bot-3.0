using Discord_Bot.Services.Models.Twitch;

namespace Discord_Bot.Interfaces.Services
{
    public interface ITwitchAPI
    {
        void Start();
        UserData GetChannel(string username);
    }
}
