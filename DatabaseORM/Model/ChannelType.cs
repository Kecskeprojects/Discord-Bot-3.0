using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class ChannelType
{
    public int ChannelTypeId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();
}
