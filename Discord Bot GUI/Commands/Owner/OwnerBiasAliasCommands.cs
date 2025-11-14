using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Owner;

[Name("Bias Alias")]
[Remarks("Owner")]
[Summary("Managing aliases (alternative names) for idols")]
public class OwnerBiasAliasCommands(
    IIdolAliasService idolAliasService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IIdolAliasService idolAliasService = idolAliasService;

    [Command("bias alias add")]
    [RequireOwner]
    [Summary("Adding a new alias for an existing idol")]
    public async Task AddBiasAlias([Name("alias-stage name-group")][Remainder] string parameters)
    {
        try
        {
            string[] paramArray = GetParametersBySplit(parameters, '-');
            if (paramArray.Length != 3)
            {
                return;
            }

            string biasAlias = paramArray[0];
            string biasName = paramArray[1];
            string biasGroup = paramArray[2];

            if (string.IsNullOrEmpty(biasName) || string.IsNullOrEmpty(biasGroup))
            {
                return;
            }

            DbProcessResultEnum result = await idolAliasService.AddIdolAliasAsync(biasAlias, biasName, biasGroup);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Bias alias added to list.",
                DbProcessResultEnum.AlreadyExists => "Bias alias already in database.",
                DbProcessResultEnum.NotFound => "Bias with that name not found in database.",
                _ => "Bias alias could not be added!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerBiasAliasCommands.cs AddBiasAlias", ex);
        }
    }

    [Command("bias alias remove")]
    [RequireOwner]
    [Summary("Removing a specific alias tied to a specific idol")]
    public async Task RemoveBiasAlias([Name("alias-stage name-group")][Remainder] string parameters)
    {
        try
        {
            string[] paramArray = GetParametersBySplit(parameters, '-');
            if (paramArray.Length != 3)
            {
                return;
            }

            string biasAlias = paramArray[0];
            string biasName = paramArray[1];
            string biasGroup = paramArray[2];

            if (string.IsNullOrEmpty(biasName) || string.IsNullOrEmpty(biasGroup))
            {
                return;
            }

            DbProcessResultEnum result = await idolAliasService.RemoveIdolAliasAsync(biasAlias, biasName, biasGroup);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Bias alias removed from list.",
                DbProcessResultEnum.NotFound => "Bias alias not in database.",
                _ => "Bias alias could not be removed!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerBiasAliasCommands.cs RemoveBiasAlias", ex);
        }
    }
}
