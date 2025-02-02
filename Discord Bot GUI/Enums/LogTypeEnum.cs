namespace Discord_Bot.Enums;

public enum LogTypeEnum
{
    Log = 1,
    Query = 2,
    Client = 3,
    MesUser = 4,
    MesOther = 5,
    Error = 6,
    Warning = 7
}

public static class LogTypeEnumExtension
{
    public static string ToLogFriendlyString(this LogTypeEnum logType)
    {
        return logType switch
        {
            LogTypeEnum.Log => "LOG",
            LogTypeEnum.Query => "QUERY",
            LogTypeEnum.Client => "CLIENT",
            LogTypeEnum.MesUser => "MES_USER",
            LogTypeEnum.MesOther => "MES_OTHER",
            LogTypeEnum.Error => "ERROR",
            LogTypeEnum.Warning => "WARNING",
            _ => "UNSPECIFIED"
        };
    }
}
