using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class BiasOwnerCommands(ICoreLogic coreLogic, IIdolService idolService, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), IBiasOwnerCommands
    {
        private readonly ICoreLogic coreLogic = coreLogic;
        private readonly IIdolService idolService = idolService;

        [Command("biaslist add")]
        [RequireOwner]
        [Summary("Admin command for adding a new bias into our lists")]
        public async Task AddBiasList([Remainder] string biasData)
        {
            try
            {
                string biasName = biasData.ToLower().Split('-')[0].Trim();
                string biasGroup = biasData.ToLower().Split('-')[1].Trim();

                if (string.IsNullOrEmpty(biasName) || string.IsNullOrEmpty(biasGroup))
                {
                    return;
                }

                DbProcessResultEnum result = await idolService.AddIdolAsync(biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias added to list!");
                }
                else if (result == DbProcessResultEnum.AlreadyExists)
                {
                    await ReplyAsync("Bias already in database!");
                }
                else
                {
                    await ReplyAsync("Bias could not be added!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasOwnerCommands.cs AddBiasList", ex.ToString());
            }
        }

        [Command("biaslist remove")]
        [RequireOwner]
        [Summary("Admin command for removing a bias from our lists")]
        public async Task RemoveBiasList([Remainder] string biasData)
        {
            try
            {
                string biasName = biasData.ToLower().Split('-')[0].Trim();
                string biasGroup = biasData.ToLower().Split('-')[1].Trim();

                //Try removing them from the database
                DbProcessResultEnum result = await idolService.RemoveIdolAsync(biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias removed from list!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Bias could not be found.");
                }
                else
                {
                    await ReplyAsync("Bias could not be removed!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasOwnerCommands.cs RemoveBiasList", ex.ToString());
            }
        }

        [Command("manual update bias")]
        [RequireOwner]
        [Summary("Update the extended information of idols manually from www.dbkpop.com")]
        public async Task ManualUpdateBias()
        {
            try
            {
                await coreLogic.UpdateExtendedBiasData();
            }
            catch (Exception ex)
            {
                logger.Error("BiasOwnerCommands.cs ManualUpdateBias", ex.ToString());
            }
        }
    }
}
