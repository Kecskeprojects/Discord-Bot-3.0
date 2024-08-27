using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Tools;
using System;
using System.IO;
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

    [Command("bonk")]
    [Alias(["hornyjail, hit me please"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Inserts a gif with the user's local profile picture, if no user is given, it will be yourself")]
    public async Task Bonk([Name("user name")] IUser user = null, [Name("delay(ms)")] int framedelay = 10)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText))
            {
                return;
            }

            string url = "";
            if (user == null)
            {
                url = GetCurrentUserAvatar();
            }
            else
            {
                await Context.Guild.DownloadUsersAsync();

                url = DiscordTools.GetUserAvatarUrl(user);
            }

            if (!string.IsNullOrEmpty(url))
            {
                logger.Query($"Getting profile image:\n{url}");
                using (MemoryStream stream = await WebTools.GetStream(url))
                using (MemoryStream gifStream = bonkGifProcessor.CreateBonkImage(stream, framedelay))
                {
                    await Context.Channel.SendFileAsync(gifStream, $"bonk_{user.Username}.gif");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserBonkCommands.cs Bonk", ex);
        }
    }
}
