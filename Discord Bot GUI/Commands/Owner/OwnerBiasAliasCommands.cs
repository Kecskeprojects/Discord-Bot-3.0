﻿using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Owner;

public class OwnerBiasAliasCommands(
    IIdolAliasService idolAliasService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IIdolAliasService idolAliasService = idolAliasService;

    [Command("bias alias add")]
    [RequireOwner]
    [Summary("Admin command for adding a new bias alias into our lists")]
    public async Task AddBiasAlias([Remainder] string biasData)
    {
        try
        {
            string biasAlias = biasData.ToLower().Split('-')[0].Trim();
            string biasName = biasData.ToLower().Split('-')[1].Trim();
            string biasGroup = biasData.ToLower().Split('-')[2].Trim();

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
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerBiasAliasCommands.cs AddBiasAlias", ex);
        }
    }

    [Command("bias alias remove")]
    [RequireOwner]
    [Summary("Admin command for removing a bias alias from our lists")]
    public async Task RemoveBiasAlias([Remainder] string biasData)
    {
        try
        {
            string biasAlias = biasData.ToLower().Split('-')[0].Trim();
            string biasName = biasData.ToLower().Split('-')[1].Trim();
            string biasGroup = biasData.ToLower().Split('-')[2].Trim();

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
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerBiasAliasCommands.cs RemoveBiasAlias", ex);
        }
    }
}
