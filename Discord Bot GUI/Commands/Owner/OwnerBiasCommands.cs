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

namespace Discord_Bot.Commands.Owner;

[Name("Bias")]
[Remarks("Owner")]
[Summary("Managing idols")]
public class OwnerBiasCommands(
    IIdolService idolService,
    IServerService serverService,
    BiasScrapingProcessor biasScrapingProcessor,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IIdolService idolService = idolService;
    private readonly BiasScrapingProcessor biasScrapingProcessor = biasScrapingProcessor;

    [Command("biaslist add")]
    [RequireOwner]
    [Summary("Adding a new idol to the database")]
    public async Task AddBiasList([Name("stage name-group")][Remainder] string parameters)
    {
        try
        {
            string[] paramArray = GetParametersBySplit(parameters, '-');
            if (paramArray.Length != 2)
            {
                return;
            }

            string biasName = paramArray[0];
            string biasGroup = paramArray[1];

            if (string.IsNullOrEmpty(biasName) || string.IsNullOrEmpty(biasGroup))
            {
                return;
            }

            DbProcessResultEnum result = await idolService.AddIdolAsync(biasName, biasGroup);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Bias added to list.",
                DbProcessResultEnum.AlreadyExists => "Bias already in database.",
                _ => "Bias could not be added!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerBiasCommands.cs AddBiasList", ex);
        }
    }

    [Command("biaslist remove")]
    [RequireOwner]
    [Summary("Removing an idol and related data from database")]
    public async Task RemoveBiasList([Name("stage name-group")][Remainder] string parameters)
    {
        try
        {
            string[] paramArray = GetParametersBySplit(parameters, '-');
            if (paramArray.Length != 2)
            {
                return;
            }

            string biasName = paramArray[0];
            string biasGroup = paramArray[1];

            //Try removing them from the database
            DbProcessResultEnum result = await idolService.RemoveIdolAsync(biasName, biasGroup);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Bias removed from list.",
                DbProcessResultEnum.NotFound => "Bias could not be found.",
                _ => "Bias could not be removed!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerBiasCommands.cs RemoveBiasList", ex);
        }
    }

    [Command("edit biasdata")]
    [Alias(["editbiasdata", "edit bias data", "edit bias", "editbias", "editidol", "edit idol", "editidoldata", "edit idol data", "edit idoldata"])]
    [RequireOwner]
    [Summary("Edit all existing data of an idol")]
    public async Task EditBiasData([Name("stage name-group")][Remainder] string parameters)
    {
        try
        {
            //Make the name lowercase and clear and accidental spaces
            string[] paramArray = GetParametersBySplit(parameters, '-');
            if (paramArray.Length != 2)
            {
                return;
            }

            string biasName = paramArray[0];
            string biasGroup = paramArray[1];

            MessageComponent component = EditBiasDataMessageProcessor.CreateComponent(biasName, biasGroup);

            await ReplyAsync("What action would you like to perform", components: component);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerBiasCommands.cs EditBiasData", ex);
        }
    }

    [Command("manual update bias")]
    [Alias(["manual update idol", "mass update bias", "mass update idol"])]
    [RequireOwner]
    [Summary("Start update process of extended information for idols")]
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
}
