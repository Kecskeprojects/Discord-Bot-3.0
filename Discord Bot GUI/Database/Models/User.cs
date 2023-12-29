using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class User
{
    public int UserId { get; set; }

    public string DiscordId { get; set; }

    public string LastFmusername { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<Birthday> Birthdays { get; set; } = new List<Birthday>();

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual ICollection<UserIdolStatistic> UserIdolStatistics { get; set; } = new List<UserIdolStatistic>();

    public virtual ICollection<Idol> Idols { get; set; } = new List<Idol>();
}
