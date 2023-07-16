using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Reminder
{
    public int ReminderId { get; set; }

    public int UserId { get; set; }

    public DateTime Date { get; set; }

    public string Message { get; set; }

    public virtual User User { get; set; }
}
