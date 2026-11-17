using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class Reminder
{
    public int ReminderId { get; set; }

    public int UserId { get; set; }

    public DateTime Date { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual User User { get; set; } = null!;
}
