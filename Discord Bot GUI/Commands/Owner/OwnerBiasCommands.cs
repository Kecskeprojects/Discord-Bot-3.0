using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors;
using Discord_Bot.Processors.MessageProcessor;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Owner
{
    public class OwnerBiasCommands(
        IIdolService idolService,
        IServerService serverService,
        BiasScrapingProcessor biasScrapingProcessor,
        Logging logger,
        Config config) : BaseCommand(logger, config, serverService)
    {
        private readonly IIdolService idolService = idolService;
        private readonly BiasScrapingProcessor biasScrapingProcessor = biasScrapingProcessor;

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
                logger.Error("OwnerBiasCommands.cs AddBiasList", ex);
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
                logger.Error("OwnerBiasCommands.cs RemoveBiasList", ex);
            }
        }

        [Command("manual update bias")]
        [Alias(["manual update idol", "mass update bias", "mass update idol"])]
        [RequireOwner]
        [Summary("Update the extended information of idols manually from www.dbkpop.com")]
        public async Task ManualUpdateBias()
        {
            try
            {
                await biasScrapingProcessor.RunUpdateBiasDataAsync();
            }
            catch (Exception ex)
            {
                logger.Error("OwnerBiasCommands.cs ManualUpdateBias", ex);
            }
        }

        [Command("edit biasdata")]
        [Alias(["editbiasdata", "edit bias data", "edit bias", "editbias", "editidol", "edit idol", "editidoldata", "edit idol data", "edit idoldata"])]
        [RequireOwner]
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

                MessageComponent component = EditBiasDataMessageProcessor.CreateComponent(biasName, biasGroup);

                await ReplyAsync("What action would you like to perform", components: component);
            }
            catch (Exception ex)
            {
                logger.Error("OwnerBiasCommands.cs EditBiasData", ex);
            }
        }
    }
}
