using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Birthday
{
    public int BirthdayId { get; set; }

    public int ServerId { get; set; }

    public int UserId { get; set; }

    public DateOnly Date { get; set; }

    public virtual Server Server { get; set; }

    public virtual User User { get; set; }
}
