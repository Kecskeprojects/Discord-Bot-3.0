using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class ServerMutedUser
{
    public int ServerId { get; set; }

    public int UserId { get; set; }

    public DateTime MutedUntil { get; set; }

    public string RemovedRoleDiscordIds { get; set; }

    public virtual Server Server { get; set; }

    public virtual User User { get; set; }
}
