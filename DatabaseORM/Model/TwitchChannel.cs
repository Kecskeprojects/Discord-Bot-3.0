using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class TwitchChannel
{
    public int TwitchChannelId { get; set; }

    public string TwitchId { get; set; } = null!;

    public string TwitchLink { get; set; } = null!;

    public string TwitchName { get; set; } = null!;

    public int ServerId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; } = null!;
}
