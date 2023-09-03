using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Server
{
    public int ServerId { get; set; }

    public string DiscordId { get; set; }

    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();

    public virtual ICollection<CustomCommand> CustomCommands { get; set; } = new List<CustomCommand>();

    public virtual ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<TwitchChannel> TwitchChannels { get; set; } = new List<TwitchChannel>();
}
