using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Discord_Bot.CommandsService
{
    public class RoleService
    {
        public static string CreateRoleMessage(List<RoleResource> roles)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string[] roleStrings = [.. roles.Select(r => $"- `{textInfo.ToTitleCase(r.RoleName)}`").Order()];

            string mes = "Start the message with `+` or `-` to add/remove roles (roles are not case sensitive)\n\n" +
                         "The current list of roles:\n" +
                         string.Join("\n", roleStrings);
            return mes;
        }
    }
}
