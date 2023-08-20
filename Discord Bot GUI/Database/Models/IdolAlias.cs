using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class IdolAlias
{
    public int IdolAliasId { get; set; }

    public string Alias { get; set; }

    public int IdolId { get; set; }

    public virtual Idol Idol { get; set; }
}
