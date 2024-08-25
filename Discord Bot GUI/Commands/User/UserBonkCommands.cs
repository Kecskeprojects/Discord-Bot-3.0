using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Bonk")]
[Remarks("User")]
[Summary("Bonk")]
public class UserBonkCommands(
    BonkGifProcessor bonkGifProcessor,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly BonkGifProcessor bonkGifProcessor = bonkGifProcessor;

    //Todo: Check if bonk command could work with gif profiles images
    //Todo: This command could also use the !say solution if it works for users
    [Command("bonk")]
    [Alias(["hornyjail, hit me please"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Inserts a gif with the user's local profile picture, if no user is given, it will be yourself")]
    public async Task Bonk([Name("username  delay(ms)")][Remainder] string parameters = "")
    {
        try
        {
            List<string> paramParts = [.. parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
            string userName = "";
            int frameDelay = 10;
            if (paramParts.Count > 0)
            {
                if (int.TryParse(paramParts[^1], out frameDelay))
                {
                    if (frameDelay is < 1 or > 1000)
                    {
                        await ReplyAsync("Invalid frame delay length. (1-1000)");
                        return;
                    }
                }
                else
                {
                    frameDelay = 10;
                }
                paramParts.Remove(frameDelay.ToString());
                userName = string.Join(" ", paramParts);
            }

            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText))
            {
                return;
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
                    url = DiscordTools.GetUserAvatarUrl(user);
                }
                else
                {
                    IReadOnlyCollection<RestGuildUser> users = await Context.Guild.SearchUsersAsync(userName, 1);
                    if (users.Count > 0)
                    {
                        RestGuildUser user = users.First();
                        url = DiscordTools.GetUserAvatarUrl(user);
                    }
                }
            }

            if (!string.IsNullOrEmpty(url))
            {
                logger.Query($"Getting profile image:\n{url}");
                using (MemoryStream stream = await WebTools.GetStream(url))
                using (MemoryStream gifStream = bonkGifProcessor.CreateBonkImage(stream, frameDelay))
                {
                    await Context.Channel.SendFileAsync(gifStream, $"bonk_{userName}.gif");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserBonkCommands.cs Bonk", ex);
        }
    }
}
