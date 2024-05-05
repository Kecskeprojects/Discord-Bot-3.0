using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class BiasGameCommands(Logging logger, Config config, IServerService serverService, IUserService userService) : BaseCommand(logger, config, serverService)
    {
        private readonly IUserService userService = userService;

        [Command("bias game")]
        [Alias(["bg", "biasgame"])]
        [Summary("A game of choosing favorites")]
        public async Task BiasGame()
        {
            try
            {
                if (Global.BiasGames.TryGetValue(Context.User.Id, out BiasGameData data))
                {
                    if (data.StartedAt > DateTime.UtcNow.AddMinutes(-30))
                    {
                        await ReplyAsync("You already have a game going!");
                        return;
                    }
                    Global.BiasGames.TryRemove(Context.User.Id, out _);
                }

                Global.BiasGames.TryAdd(Context.User.Id, new BiasGameData(Context.User.Id));

                EmbedBuilder mainEmbed = new();
                mainEmbed.WithTitle("Bias Game Setup");

                EmbedFooterBuilder footer = new();
                footer.WithIconUrl(Context.User.GetDisplayAvatarUrl(ImageFormat.Jpeg, 512));
                footer.WithText(Global.GetNickName(Context.Channel, Context.User));
                mainEmbed.WithFooter(footer);

                mainEmbed.AddField("1. Select a gender", "Male, Female, Both");
                mainEmbed.AddField("2. Select a debut range", "A start date and an end date");

                ActionRowBuilder buttonRow = new();
                buttonRow.WithButton(
                    emote: new Emoji("\U0001F57A"), //Man
                    customId: $"BiasGame_Setup_Gender_1_{Context.User.Id}",
                    style: ButtonStyle.Primary);
                buttonRow.WithButton(
                    emote: new Emoji("\U0001F483"), //Woman
                    customId: $"BiasGame_Setup_Gender_2_{Context.User.Id}",
                    style: ButtonStyle.Primary);
                buttonRow.WithButton(
                    emote: new Emoji("\U0001F46B"), //Both
                    customId: $"BiasGame_Setup_Gender_3_{Context.User.Id}",
                    style: ButtonStyle.Primary);

                ComponentBuilder components = new();
                components.AddRow(buttonRow);

                await ReplyAsync("", components: components.Build(), embed: mainEmbed.Build());
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameCommands.cs BiasGame", ex.ToString());
            }
        }

        [Command("bias game stop")]
        [Alias(["bgs", "biasgamestop"])]
        [Summary("Stop current game")]
        public async Task BiasGameStop()
        {
            try
            {
                if (Global.BiasGames.TryRemove(Context.User.Id, out _))
                {
                    await ReplyAsync("Game removed!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameCommands.cs BiasGameStop", ex.ToString());
            }
        }

        //Todo: Show with some icon if the idol is currently added to their biases
        [Command("bias stats")]
        [Alias(["bs", "biasstats", "biastats", "bias stat", "biasstat", "biastat"])]
        [Summary("Show your game statistics")]
        public async Task BiasStats()
        {
            try
            {
                UserBiasGameStatResource stats = await userService.GetTopIdolsAsync(Context.User.Id, GenderType.None);

                if (stats == null || stats.BiasGameCount == 0 || stats.Stats.Count == 0)
                {
                    await ReplyAsync("You have not played any games!");
                    return;
                }

                Embed[] embed = BiasGameEmbedProcessor.CreateEmbed(Global.GetNickName(Context.Channel, Context.User), GenderType.None, stats);

                MessageComponent component = BiasGameEmbedProcessor.CreateComponent(GenderType.None, Context.User.Id);

                await ReplyAsync(embeds: embed, components: component);
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameCommands.cs BiasStats", ex.ToString());
            }
        }
    }
}
