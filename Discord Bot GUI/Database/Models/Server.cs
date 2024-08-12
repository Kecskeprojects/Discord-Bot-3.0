using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Server
{
    public int ServerId { get; set; }

    public string DiscordId { get; set; }

    public int? NotificationRoleId { get; set; }

    public string RoleMessageDiscordId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public int? MuteRoleId { get; set; }

    public virtual ICollection<Birthday> Birthdays { get; set; } = new List<Birthday>();

    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();

    public virtual ICollection<CustomCommand> CustomCommands { get; set; } = new List<CustomCommand>();

    public virtual ICollection<EmbedGroup> EmbedGroups { get; set; } = new List<EmbedGroup>();

    public virtual ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();

    public virtual Role MuteRole { get; set; }

    public virtual Role NotificationRole { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<ServerMutedUser> ServerMutedUsers { get; set; } = new List<ServerMutedUser>();

    public virtual ICollection<TwitchChannel> TwitchChannels { get; set; } = new List<TwitchChannel>();
}
