using Discord;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IServiceDiscordCommunication
    {
        Task SendTwitchEmbed(TwitchChannelResource channel, string thumbnailUrl, string title);
        Task<string> SendTwitterMessage(List<Uri> videos, List<Uri> images, ulong channelId, MessageReference messageReference, string messages);
        Task<bool> YoutubeAddPlaylistMessage(ulong channelId);
    }
}
