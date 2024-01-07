using Discord_Bot.Core.Config;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord.Interactions;
using System.Threading.Tasks;
using System;

namespace Discord_Bot.Interactions
{
    public class BiasOwnerSlashCommands(Logging logger, Config config, IIdolService idolService) : BaseInteraction(logger, config)
    {
        private readonly IIdolService idolService = idolService;
        //Todo: Command to edit idol data manually, with an option to only change the profile link, command to change which group idol belongs to, command to edit group manually

        [SlashCommand("bias edit", "Edit an idol's details")]
        public async Task BiasEdit()
        {
            try
            {

            }
            catch (Exception ex)
            {
                logger.Error("BiasOwnerSlashCommands.cs IdolEdit", ex.ToString());
            }
        }
    }
}
