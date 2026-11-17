using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class Channel
{
    public int ChannelId { get; set; }

    public int ServerId { get; set; }

    public string DiscordId { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; } = null!;

    public virtual ICollection<WeeklyPoll> WeeklyPolls { get; set; } = new List<WeeklyPoll>();

    public virtual ICollection<ChannelType> ChannelTypes { get; set; } = new List<ChannelType>();
}
