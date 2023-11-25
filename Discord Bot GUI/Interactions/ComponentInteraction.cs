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
    public class ComponentInteraction(Logging logger, Config config, IIdolService biasService) : BaseInteraction(logger, config)
    {
        private readonly IIdolService biasService = biasService;

        [ComponentInteraction("biasMenu_*")]
        public async Task BiasMenuHandler(string[] selectedBiasGroups)
        {
            try
            {
                List<IdolResource> biases;
                if (selectedBiasGroups[0].Contains("><"))
                {
                    string name = selectedBiasGroups[0].Split("><")[0];
                    ulong userId = ulong.Parse(selectedBiasGroups[0].Split("><")[1]);
                    biases = await biasService.GetUserBiasesByGroupAsync(name, userId);
                }
                else
                {
                    biases = await biasService.GetBiasesByGroupAsync(selectedBiasGroups[0]);
                }

                string message = "";

                //Add Group name
                message += $"{selectedBiasGroups[0].Split("><")[0].ToUpper()}:\n";

                //Add individual members
                foreach (IdolResource member in biases)
                {
                    if (member != biases[0])
                    {
                        message += ", ";
                    }

                    message += $"`{member.Name.ToUpper()}`";
                }

                await Context.Interaction.RespondAsync(message);
            }
            catch (Exception ex)
            {
                logger.Error("ComponentInteraction.cs BiasMenuHandler", ex.ToString());
            }
        }
    }
}
