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

                FileAttachment file = new(Global.GetStream("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSYscfUBUbqwGd_DHVhG-ZjCOD7MUpxp4uhNe7toUg4ug&s"), $"img.jpg");
                mainEmbed.WithImageUrl($"attachment://img.jpg");

                RestUserMessage message = await Context.Channel.SendFileAsync(file, embed: mainEmbed.Build());

                await Task.Delay(2000);

                mainEmbed = new();
                mainEmbed.WithTitle("Bias Game 2/2");
                footer = new();
                footer.WithIconUrl(Context.User.GetDisplayAvatarUrl(ImageFormat.Jpeg, 512));
                footer.WithText(Global.GetNickName(Context.Channel, Context.User));
                mainEmbed.WithFooter(footer);

                file = new(Global.GetStream("https://dfstudio-d420.kxcdn.com/wordpress/wp-content/uploads/2019/06/digital_camera_photo-1080x675.jpg"), $"img2.jpg");
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
