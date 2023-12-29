using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Channel
{
    public int ChannelId { get; set; }

    public int ServerId { get; set; }

    public string DiscordId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; }

    public virtual ICollection<ChannelType> ChannelTypes { get; set; } = new List<ChannelType>();
}
