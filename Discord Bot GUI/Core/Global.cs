using Discord;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_Bot.Core
{
    //Todo: remove all functions from here when it becomes possible, these list may be placed somewhere else, or put into some form of singleton service
    public static class Global
    {
        public static ConcurrentDictionary<ulong, ServerAudioResource> ServerAudioResources { get; private set; } = [];
        public static ConcurrentDictionary<ulong, BiasGameData> BiasGames { get; private set; } = [];

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

        public static async Task<Stream> GetStream(string url)
        {
            Stream imageData = null;

            using (HttpClient wc = new()
            {
                Timeout = new TimeSpan(0, 3, 0)
            })
            {
                imageData = await wc.GetStreamAsync(url);
            }

            return imageData;
        }
    }
}
