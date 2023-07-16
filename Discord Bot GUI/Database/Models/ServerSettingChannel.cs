using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class ServerSettingChannel
{
    public int ServerSettingChannelId { get; set; }

    public int ChannelId { get; set; }

    public int ChannelTypeId { get; set; }

    public virtual Channel Channel { get; set; }

    public virtual ChannelType ChannelType { get; set; }
}
