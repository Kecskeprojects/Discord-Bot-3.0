using Discord_Bot.Services.Models.Twitch;

namespace Discord_Bot.Interfaces.Services;
public interface ITwitchCLI
{
    UserData GetChannel(string username);
    public string GenerateToken();
}
