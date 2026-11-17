using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class IdolImage
{
    public int ImageId { get; set; }

    public int IdolId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public string? OverriddenUrl { get; set; }

    public virtual Idol Idol { get; set; } = null!;
}
