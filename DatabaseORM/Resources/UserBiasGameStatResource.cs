namespace DatabaseORM.Resources;

public class UserBiasGameStatResource
{
    public int UserId { get; set; }

    public ulong DiscordId { get; set; }

    public int BiasGameCount { get; set; }

    public List<UserIdolStatisticResource> Stats { get; set; } = [];
}
