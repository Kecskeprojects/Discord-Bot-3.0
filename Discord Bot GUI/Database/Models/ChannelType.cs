using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class ChannelType
{
    public int ChannelTypeId { get; set; }

    public string Name { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();
}
