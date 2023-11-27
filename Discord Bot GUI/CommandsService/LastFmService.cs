using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.CommandsService
{
    public class LastFmService
    {
        public static EmbedBuilder BaseEmbed(string HeadText, string image_url = "")
        {
            //Building embed
            EmbedBuilder builder = new();

            builder.WithAuthor(HeadText, iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");

            if (image_url != "")
            {
                builder.WithThumbnailUrl(image_url);
            }

            builder.WithCurrentTimestamp();
            builder.WithColor(Color.Red);

            return builder;
        }

        public static List<UserResource> FilterToOnlyServerMembers(SocketCommandContext context, List<UserResource> users)
        {
            List<UserResource> filtered = [];
            foreach (UserResource item in users)
            {
                //Check if user is in given server
                Discord.WebSocket.SocketGuildUser temp_user = context.Guild.GetUser(item.DiscordId);
                if (temp_user != null)
                {
                    //Get their nickname if they have one
                    item.Username = Global.GetNickName(context.Channel, temp_user);
                    filtered.Add(item);
                }
            }

            return filtered;
        }
    }
}
