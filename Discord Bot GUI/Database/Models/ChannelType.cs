using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class ChannelType
{
    public int ChannelTypeId { get; set; }

    public string Name { get; set; }

    public virtual ICollection<ServerSettingChannel> ServerSettingChannels { get; set; } = new List<ServerSettingChannel>();
}
