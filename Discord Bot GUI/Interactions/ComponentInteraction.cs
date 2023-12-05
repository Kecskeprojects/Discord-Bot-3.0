using Discord.Interactions;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions
{
    public class ComponentInteraction(Logging logger, Config config, IIdolService idolService) : BaseInteraction(logger, config)
    {
        private readonly IIdolService idolService = idolService;

        [ComponentInteraction("biasMenu_*")]
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
                    idoles = await idolService.GetUserIdolsListAsync(userId, name);
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

                await Context.Interaction.RespondAsync(message);
            }
            catch (Exception ex)
            {
                logger.Error("ComponentInteraction.cs IdolMenuHandler", ex.ToString());
            }
        }
    }
}
