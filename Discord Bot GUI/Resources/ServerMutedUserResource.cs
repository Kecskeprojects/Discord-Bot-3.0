using System;

namespace Discord_Bot.Resources;
public class ServerMutedUserResource
{
    public string ServerDiscordId { get; set; }

    public string UserDiscordId { get; set; }

    public DateTime MutedUntil { get; set; }

    public string RemovedRoleDiscordIds { get; set; }
}
