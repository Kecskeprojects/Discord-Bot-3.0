using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Idol
{
    public int IdolId { get; set; }

    public string Name { get; set; }

    public int GroupId { get; set; }

    public virtual IdolGroup Group { get; set; }

    public virtual ICollection<IdolAlias> IdolAliases { get; set; } = new List<IdolAlias>();

    public virtual ICollection<UserBias> UserBiases { get; set; } = new List<UserBias>();
}
