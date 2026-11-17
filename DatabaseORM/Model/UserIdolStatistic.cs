using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class UserIdolStatistic
{
    public int UserId { get; set; }

    public int IdolId { get; set; }

    public int Placed1 { get; set; }

    public int Placed2 { get; set; }

    public int Placed3 { get; set; }

    public int Placed4 { get; set; }

    public int Placed5 { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public virtual Idol Idol { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
