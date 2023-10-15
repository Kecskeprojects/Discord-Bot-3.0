using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class TwitchChannel
{
    public int TwitchChannelId { get; set; }

    public string TwitchId { get; set; }

    public string TwitchLink { get; set; }

    public string TwitchName { get; set; }

    public int ServerId { get; set; }

    public virtual Server Server { get; set; }
}
