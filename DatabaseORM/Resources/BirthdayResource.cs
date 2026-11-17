namespace DatabaseORM.Resources;

public class BirthdayResource
{
    public int BirthdayId { get; set; }

    public ulong ServerDiscordId { get; set; }

    public ulong UserDiscordId { get; set; }

    public DateOnly Date { get; set; }
}
