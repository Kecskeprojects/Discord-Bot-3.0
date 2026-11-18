using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Tools;
using System.IO;

namespace Discord_Bot.Commands.User;

[Name("Bonk")]
[Remarks("User")]
[Summary("Bonk")]
public class UserBonkCommands(
    BonkGifProcessor bonkGifProcessor,
    IWebService webService,
    IServerService serverService,
    IBotLogger logger,
    IConfig config) : BaseCommand(logger, config, serverService)
{
    private readonly BonkGifProcessor bonkGifProcessor = bonkGifProcessor;
    private readonly IWebService webService = webService;

    [Command("bonk")]
    [Alias(["hornyjail, hit me please"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Inserts a gif with the user's local profile picture, if no user is given, it will be yourself")]
    public async Task Bonk([Remainder][Name("username  delay(ms)")] string parameters = "")
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText))
            {
                return;
            }

            List<string> paramParts = [.. parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
            string userName = "";
            int frameDelay = 10;
            if (paramParts.Count > 0)
            {
                if (int.TryParse(paramParts[^1], out frameDelay))
                {
                    if (frameDelay is < 1 or > 1000)
                    {
                        _ = await ReplyAsync("Invalid frame delay length. (1-1000)");
                        return;
                    }
                }
                else
                {
                    frameDelay = 10;
                }
                _ = paramParts.Remove(frameDelay.ToString());
                userName = string.Join(" ", paramParts);
            }

            string url = "";
            if (string.IsNullOrEmpty(userName))
            {
                url = GetCurrentUserAvatar();
                userName = Context.User.Username;
            }
            else
            {
                await Context.Guild.DownloadUsersAsync();

                if (ulong.TryParse(userName, out ulong userId))
                {
                    SocketGuildUser user = Context.Guild.GetUser(userId);
                    userName = user.Username;
                    url = DiscordTools.GetUserAvatarUrl(user);
                }
                else
                {
                    IReadOnlyCollection<RestGuildUser> users = await Context.Guild.SearchUsersAsync(userName, 1);
                    if (users.Count > 0)
                    {
                        RestGuildUser user = users.First();
                        userName = user.Username;
                        url = DiscordTools.GetUserAvatarUrl(user);
                    }
                }
            }

            if (!string.IsNullOrEmpty(url))
            {
                logger.Query($"Getting profile image:\n{url}");
                using (MemoryStream stream = await webService.GetStream(url))
                using (MemoryStream? gifStream = bonkGifProcessor.CreateBonkImage(stream, frameDelay))
                {
                    //Null when the gif could not be made, the processor already logged why
                    if (gifStream != null)
                    {
                        _ = await Context.Channel.SendFileAsync(gifStream, $"bonk_{userName}.gif");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserBonkCommands.cs Bonk", ex);
        }
    }
}
