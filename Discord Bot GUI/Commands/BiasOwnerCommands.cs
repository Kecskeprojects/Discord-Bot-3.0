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
        [Alias(["manual update idol", "mass update bias", "mass update idol"])]
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

        [Command("edit biasdata")]
        [RequireOwner]
        [Alias(["editbiasdata", "edit bias data", "edit bias", "editbias", "editidol", "edit idol", "editidoldata", "edit idol data", "edit idoldata"])]
        [Summary("Edit bias related data")]
        public async Task EditBiasData([Remainder] string biasData)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                string biasName = "";
                string biasGroup = "";

                if (biasData.Contains('-'))
                {
                    biasName = biasData.ToLower().Split('-')[0].Trim();
                    biasGroup = biasData.ToLower().Split('-')[1].Trim();
                }
                else
                {
                    biasName = biasData.ToLower().Trim();
                }

                if (biasData.Split("-").Length > 2 || string.IsNullOrWhiteSpace(biasName) || string.IsNullOrWhiteSpace(biasGroup))
                {
                    return;
                }

                SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Select edit type")
                    .WithCustomId("EditIdolData")
                    .AddOption("Edit Group", $"{BiasEditActionTypeEnum.EditGroup};{biasName};{biasGroup}", "Edit group currently related to idol")
                    .AddOption("Edit Idol", $"{BiasEditActionTypeEnum.EditIdol};{biasName};{biasGroup}", "Edit idol in question")
                    .AddOption("Edit Idol Extended", $"{BiasEditActionTypeEnum.EditIdolExtended};{biasName};{biasGroup}", "Edit idol's extra details")
                    .AddOption("Change Group", $"{BiasEditActionTypeEnum.ChangeGroup};{biasName};{biasGroup}", "Change the group the idol belongs to")
                    .AddOption("Change Profile Link", $"{BiasEditActionTypeEnum.ChangeProfileLink};{biasName};{biasGroup}", "Edit idol's profile page link")
                    .AddOption("Remove Images", $"{BiasEditActionTypeEnum.RemoveImage};{biasName};{biasGroup}", "Remove all images stored for idol");

                ComponentBuilder builder = new();
                builder.WithSelectMenu(menuBuilder);

                await ReplyAsync("What action would you like to perform", components: builder.Build());
            }
            catch (Exception ex)
            {
                logger.Error("BiasOwnerCommands.cs EditBiasData", ex.ToString());
            }
        }
    }
}
