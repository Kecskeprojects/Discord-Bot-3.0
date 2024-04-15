using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class BiasGameCommands(Logging logger, Config config, IServerService serverService) : BaseCommand(logger, config, serverService)
    {
        //Todo: Create full game
        //First, ask user for specifics a select for male/female/indifferent, a select/input for debut date and perhaps a select/input for when they were born
        //Perhaps another select for the number of pairs (8, 10, 12, 16(, 20, 24, if possible))
        //Some process will be needed to edit together the pictures of the "contestants", this will only be done the very first time, there is a function that relays to the interaction handler that it's a longer process
        //Second, create all combinations of first round, and create a class that will store the progress of user, this can be saved in memory into a dictionary (userId, gameClass)
        //Third, Create an InteractionHandler for the button clicks, one button will have 1, the other 2, in it's id to know which they clicked
        //------
        //Done
        //------
        //Fourth, Upon finishing a game, the results will be saved into the UserIdolStatistics table so users can check their stats of their previous games, this will be a different command
        //For reference on how the logic will work, check the bangya bot in text1
        [Command("bias game")]
        [RequireOwner]
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
                buttonRow.WithButton(emote: new Emoji("\U0001F57A"), customId: $"BiasGame_Setup_Gender_1_{Context.User.Id}", style: ButtonStyle.Primary); //Man
                buttonRow.WithButton(emote: new Emoji("\U0001F483"), customId: $"BiasGame_Setup_Gender_2_{Context.User.Id}", style: ButtonStyle.Primary); //Woman
                buttonRow.WithButton(emote: new Emoji("\U0001F46B"), customId: $"BiasGame_Setup_Gender_3_{Context.User.Id}", style: ButtonStyle.Primary); //Both

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
        [RequireOwner]
        [Summary("Stop current game")]
        public async Task BiasGameStop()
        {
            try
            {
                Global.BiasGames.TryRemove(Context.User.Id, out _);
                await ReplyAsync("Game removed!");
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameCommands.cs BiasGameStop", ex.ToString());
            }
        }
    }
}
