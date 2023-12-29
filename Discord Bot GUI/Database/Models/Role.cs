using System;
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

    public virtual ICollection<Server> Servers { get; set; } = new List<Server>();
}
