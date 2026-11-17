using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class CustomCommand
{
    public int CommandId { get; set; }

    public int ServerId { get; set; }

    public string Command { get; set; } = null!;

    public string Url { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; } = null!;
}
