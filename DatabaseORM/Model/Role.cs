using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class Role
{
    public int RoleId { get; set; }

    public int ServerId { get; set; }

    public string DiscordId { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; } = null!;

    public virtual ICollection<Server> ServerMuteRoles { get; set; } = new List<Server>();

    public virtual ICollection<Server> ServerNotificationRoles { get; set; } = new List<Server>();

    public virtual ICollection<WeeklyPoll> WeeklyPolls { get; set; } = new List<WeeklyPoll>();
}
