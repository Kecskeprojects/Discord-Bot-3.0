using Discord;
using Discord.Commands;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot
{
    public static class Global
    {
        #region Global variables
        //Might not be needed
        //Server information stored in a dictionary, the key is the Context.Guild.Id, the value is a complex class
        public static readonly Dictionary<ulong, ServerResource> servers = new();

        public static bool InstagramChecker { get; set; }
        
        public static bool TwitterChecker { get; set; }
        #endregion

        #region Global Functions
        //Returns true if command's channel is a music channel
        public static bool IsMusicChannel(SocketCommandContext context)
        {
            if (servers[context.Guild.Id].MusicChannel == 0) return true;
            else return servers[context.Guild.Id].MusicChannel == context.Channel.Id;
        }

        //Check if user has a nickname, and if message was sent in a server or not
        public static string GetNickName(SocketCommandContext context)
        {
            //Only check for nickname if user is not using DMs
            if (context.Channel.GetChannelType() != ChannelType.DM)
            {
                //If user has a nickname, use that in the embed
                return (context.User as Discord.WebSocket.SocketGuildUser).Nickname ?? context.User.Username;
            }
            else return context.User.Username;
        }

        public static Stream GetStream(string url)
        {
            Stream imageData = null;

            using (var wc = new System.Net.Http.HttpClient())
            {
                imageData = wc.GetStreamAsync(url).GetAwaiter().GetResult();
            }

            return imageData;
        }
        #endregion
    }
}
