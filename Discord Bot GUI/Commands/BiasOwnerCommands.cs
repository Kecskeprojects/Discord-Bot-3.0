using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class BiasOwnerCommands(IBiasDatabaseService biasDatabaseService, IIdolService idolService, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), IBiasOwnerCommands
    {
        private readonly IBiasDatabaseService biasDatabaseService = biasDatabaseService;
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
                await biasDatabaseService.RunUpdateBiasDataAsync();
            }
            catch (Exception ex)
            {
                logger.Error("BiasOwnerCommands.cs ManualUpdateBias", ex.ToString());
            }
        }

        //Todo: Command to edit idol data manually, with an option to only change the profile link, command to change which group idol belongs to, command to edit group manually
        //The command could be one single command that goes like this: !edit idoldata [name]-[group] that will reply with a select
        //showing the following options: idoldata, groupdata, remove images, remove images will just remove all images associated with the idol
        //the other two will bring up modals to edit the idol's data, including which group they belong to, a new group name will result in an entirely new group item being created,
        //and another to edit the idol's current group's data, if the group is reassigned either way, all their extended data will be removed to get the entirely new data from the web
        //if possible the modal fields should show the current data, if any
    }
}
