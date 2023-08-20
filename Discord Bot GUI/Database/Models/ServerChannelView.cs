using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class ServerChannelView
{
    public int? ServerId { get; set; }

    public string ServerDiscordId { get; set; }

    public int? ChannelId { get; set; }

    public string ChannelDiscordId { get; set; }

    public int? ChannelTypeId { get; set; }

    public string ChannelTypeName { get; set; }
}
