using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class IdolAlias
{
    public int IdolAliasId { get; set; }

    public string Alias { get; set; } = null!;

    public int IdolId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Idol Idol { get; set; } = null!;
}
