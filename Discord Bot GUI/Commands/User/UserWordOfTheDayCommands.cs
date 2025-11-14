using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Services.Models.Wotd;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Word of the Day")]
[Remarks("User")]
[Summary("Daily updating words and their translations using http://wotd.transparent.com/")]
public class UserWordOfTheDayCommands(
    IWordOfTheDayService wordOfTheDayService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IWordOfTheDayService wordOfTheDayService = wordOfTheDayService;

    [Command("wotd")]
    [Alias(["word of the day"])]
    [Summary("Check the word of the day in a language")]
    public async Task WotdFunction([Name("language name")] string language = "korean")
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            WotdBase result = await wordOfTheDayService.GetDataAsync(language);

            if (result != null)
            {
                Embed[] embed = WordOfTheDayEmbedProcessor.CreateEmbed(result);

                _ = await ReplyAsync(embeds: embed);
            }
            else
            {
                _ = await ReplyAsync("Language is not supported, here is the list of languages:\n" + string.Join(", ", Constant.WotdLanguages.Keys));
            }
        }
        catch (HttpRequestException)
        {
            _ = await ReplyAsync("Site is currently unavailable, try again in a little bit");
        }
        catch (Exception ex)
        {
            logger.Error("UserWordOfTheDayCommands.cs WotdFunction", ex);
        }
    }
}
