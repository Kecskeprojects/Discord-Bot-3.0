using Discord;
using Discord.Commands;
using Discord.Rest;
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
        //If at any point during the furter rounds a non-even number shows up, one contestant skips to the next round
        //Second, create all combinations of first round, and create a class that will store the progress of user, this can be saved in memory into a dictionary (userId, gameClass)
        //Some process will be needed to edit together the pictures of the "contestants", this will only be done the very first time, there is a function that relays to the interaction handler that it's a longer process
        //Third, Create an InteractionHandler for the button clicks, one button will have 1, the other 2, in it's id to know which they clicked, and of course, create handlers for the interactions before game start
        //Fourth, Upon finishing a game, the results will be saved into the UserIdolStatistics table so users can check their stats of their previous games, this will be a different command
        //For reference on how the logic will work, check the bangya bot in text1
        [Command("bias game")]
        [RequireOwner]
        [Summary("Update the extended information of idols manually from www.dbkpop.com")]
        public async Task BiasGame()
        {
            try
            {
                EmbedBuilder mainEmbed = new();
                mainEmbed.WithTitle("Bias Game 1/2");
                EmbedFooterBuilder footer = new();
                footer.WithIconUrl(Context.User.GetDisplayAvatarUrl(ImageFormat.Jpeg, 512));
                footer.WithText(Global.GetNickName(Context.Channel, Context.User));
                mainEmbed.WithFooter(footer);

                FileAttachment file = new(await Global.GetStream("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSYscfUBUbqwGd_DHVhG-ZjCOD7MUpxp4uhNe7toUg4ug&s"), $"img.jpg");
                mainEmbed.WithImageUrl($"attachment://img.jpg");

                RestUserMessage message = await Context.Channel.SendFileAsync(file, embed: mainEmbed.Build());

                await Task.Delay(2000);

                mainEmbed = new();
                mainEmbed.WithTitle("Bias Game 2/2");
                footer = new();
                footer.WithIconUrl(Context.User.GetDisplayAvatarUrl(ImageFormat.Jpeg, 512));
                footer.WithText(Global.GetNickName(Context.Channel, Context.User));
                mainEmbed.WithFooter(footer);

                file = new(await Global.GetStream("https://dfstudio-d420.kxcdn.com/wordpress/wp-content/uploads/2019/06/digital_camera_photo-1080x675.jpg"), $"img2.jpg");
                mainEmbed.WithImageUrl($"attachment://img2.jpg");

                await message.ModifyAsync(x =>
                {
                    x.Attachments = new[] { file };
                    x.Embed = mainEmbed.Build();
                });
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameCommands.cs BiasGame", ex.ToString());
            }
        }
    }
}
