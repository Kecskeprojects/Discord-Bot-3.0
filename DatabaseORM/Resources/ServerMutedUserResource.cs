namespace DatabaseORM.Resources;

public class ServerMutedUserResource
{
    public string ServerDiscordId { get; set; } = "";

    public string UserDiscordId { get; set; } = "";

    public DateTime MutedUntil { get; set; }

    public string RemovedRoleDiscordIds { get; set; } = "";
}
