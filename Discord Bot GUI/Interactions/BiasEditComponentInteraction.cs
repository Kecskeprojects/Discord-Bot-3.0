using Discord.Interactions;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions
{
    public class BiasEditComponentInteraction(Logging logger, Config config) : BaseInteraction(logger, config)
    {
        [ComponentInteraction("editIdolData")]
        public async Task EditIdolMenuHandler(string[] selectedAction)
        {
            try
            {
                logger.Log($"Idol menu item selected with following parameters: {string.Join(",", selectedAction)}", LogOnly: true);

                BiasEditData data = new(selectedAction[0]);

                await Context.Interaction.RespondAsync(selectedAction[0]);
            }
            catch (Exception ex)
            {
                logger.Error("BiasEditComponentInteraction.cs EditIdolMenuHandler", ex.ToString());
            }
        }
    }
}
