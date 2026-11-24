using DatabaseORM.Resources;
using System.Globalization;

namespace Discord_Bot.Processors.MessageProcessor;

public static class RoleMessageProcessor
{
    public static string CreateMessage(List<RoleResource> roles)
    {
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        //The same order on every machine: until 2026-11-24 it followed the machine's culture, and Hungarian sorts "cs" after "c"
        string[] roleStrings = [.. roles.Select(r => $"- `{textInfo.ToTitleCase(r.RoleName)}`").Order(StringComparer.InvariantCulture)];

        string mes = "Start the message with `+` or `-` to add/remove roles (roles are not case sensitive)\n\n" +
                     "The current list of roles:\n" +
                     string.Join("\n", roleStrings);
        return mes;
    }
}
