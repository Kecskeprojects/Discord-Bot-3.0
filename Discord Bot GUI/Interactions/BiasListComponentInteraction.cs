using Discord.Interactions;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions
{
    public class BiasListComponentInteraction(Logging logger, Config config, IUserIdolService userIdolService, IIdolService idolService) : BaseInteraction(logger, config)
    {
        private readonly IUserIdolService userIdolService = userIdolService;
        private readonly IIdolService idolService = idolService;

        [ComponentInteraction("BiasMenu_*")]
        public async Task IdolMenuHandler(int count, string[] selectedIdolGroups)
        {
            try
            {
                logger.Log($"Idol menu item selected with following parameters: {count}, {string.Join(",", selectedIdolGroups)}", LogOnly: true);
                if (count > 1)
                {
                    return;
                }

                List<IdolResource> idoles;
                if (selectedIdolGroups[0].Contains("><"))
                {
                    string name = selectedIdolGroups[0].Split("><")[0];
                    ulong userId = ulong.Parse(selectedIdolGroups[0].Split("><")[1]);
                    idoles = await userIdolService.GetUserIdolsListAsync(userId, name);
                }
                else
                {
                    idoles = await idolService.GetIdolsByGroupAsync(selectedIdolGroups[0]);
                }

                string message = "";

                //Add Group name
                message += $"{selectedIdolGroups[0].Split("><")[0].ToUpper()}:\n";

                //Add individual members
                foreach (IdolResource member in idoles)
                {
                    if (member != idoles[0])
                    {
                        message += ", ";
                    }

                    message += $"`{member.Name.ToUpper()}`";
                }

                await RespondAsync(message);
            }
            catch (Exception ex)
            {
                logger.Error("BiasListComponentInteraction.cs IdolMenuHandler", ex.ToString());
                await RespondAsync("Something went wrong while getting idols.");
            }
        }
    }
}
