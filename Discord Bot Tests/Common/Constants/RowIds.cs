namespace Discord_Bot.Tests.Common.Constants;

//The database keys of the tests' rows; like DiscordIds, they only have to differ from each other
public static class RowIds
{
    //For a row the database numbers itself (the LocalDB tests): an identity column takes no key of ours
    public const int New = 0;

    public const int Server = 7;
    public const int OtherServer = 8;
    public const int User = 3;
    public const int OtherUser = 4;
}
