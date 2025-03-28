﻿using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public int ServerId { get; set; }

    public string DiscordId { get; set; }

    public string RoleName { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; }

    public virtual ICollection<Server> ServerMuteRoles { get; set; } = new List<Server>();

    public virtual ICollection<Server> ServerNotificationRoles { get; set; } = new List<Server>();

    public virtual ICollection<WeeklyPoll> WeeklyPolls { get; set; } = new List<WeeklyPoll>();
}
