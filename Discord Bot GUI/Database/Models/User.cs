using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class User
{
    public int UserId { get; set; }

    public string DiscordId { get; set; }

    public string LastFmusername { get; set; }

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual ICollection<UserBias> UserBiases { get; set; } = new List<UserBias>();
}
