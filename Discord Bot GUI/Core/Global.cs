using Discord;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace Discord_Bot.Core
{
    public static class Global
    {
        #region Global Variables
        public static Dictionary<ulong, ServerAudioResource> ServerAudioResources { get; set; } = [];
        #endregion

        #region Global Functions
        //Check if user has a nickname
        public static string GetNickName(ISocketMessageChannel channel, SocketUser user)
        {
            return channel.GetChannelType() != ChannelType.DM ?
                (user as SocketGuildUser).Nickname ?? user.Username :
                user.Username;
        }

        //Check if server has a type of channel, and if yes, is it on the list
        public static bool IsTypeOfChannel(ServerResource server, ChannelTypeEnum type, ulong channelId, bool allowLackOfType = true)
        {
            return server == null || (!server.SettingsChannels.TryGetValue(type, out List<ulong> value) ?
                        allowLackOfType : value.Contains(channelId));
        }

        public static Stream GetStream(string url)
        {
            Stream imageData = null;

            using (HttpClient wc = new() { Timeout = new TimeSpan(0, 3, 0) })
            {
                imageData = wc.GetStreamAsync(url).GetAwaiter().GetResult();
            }

            return imageData;
        }

        //Testing connection by pinging google, it is quite a problem if that's down too
        public static bool Connection()
        {
            try
            {
                if (new Ping().Send("google.com", 1000, new byte[32], new PingOptions()).Status == IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }
        #endregion
    }
}
