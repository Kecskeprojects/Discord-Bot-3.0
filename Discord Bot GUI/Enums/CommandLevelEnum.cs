namespace Discord_Bot.Enums;
public enum CommandLevelEnum
{
    Owner = 1,
    Admin = 2,
    User = 3
}

public static class CommandLevelEnumExtension
{
    public static string ToFriendlyString(this CommandLevelEnum commandType)
    {
        return commandType switch
        {
            CommandLevelEnum.Owner => "Owner",
            CommandLevelEnum.Admin => "Admin",
            CommandLevelEnum.User => "User",
            _ => "Unspecified"
        };
    }
}
