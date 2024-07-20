using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class EmbedGroup
{
    public int EmbedGroupId { get; set; }

    public int ServerId { get; set; }

    public int ChannelId { get; set; }

    public int MessageId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Channel Channel { get; set; }

    public virtual ICollection<Embed> Embeds { get; set; } = new List<Embed>();

    public virtual Server Server { get; set; }
}
