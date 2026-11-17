using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class Birthday
{
    public int BirthdayId { get; set; }

    public int ServerId { get; set; }

    public int UserId { get; set; }

    public DateOnly Date { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
