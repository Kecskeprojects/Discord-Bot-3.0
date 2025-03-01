using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Database.DBServices;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Polls;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Owner;

[Name("Weekly Poll Option Preset")]
[Remarks("Owner")]
[Summary("Manage presets for Weekly Polls")]
public class OwnerWeeklyPollOptionPresetCommands(
    IWeeklyPollOptionPresetService weeklyPollOptionPresetService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IWeeklyPollOptionPresetService weeklyPollOptionPresetService = weeklyPollOptionPresetService;

    [Command("weekly poll preset create")]
    [Alias(["wpp c", "weekly poll preset c", "wpp create", "weeklypollpreset c", "weeklypollpreset create"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Adding a new Weekly Poll Option Preset")]
    public async Task AddWeeklyPollOptionPreset()
    {
        try
        {
            WeeklyPollOptionPresetResource resource = await weeklyPollOptionPresetService.GetOrCreateDummyPresetAsync();

            Embed[] embeds = PollPresetEditEmbedProcessor.CreateEmbed(resource, false);
            MessageComponent component = PollPresetEditEmbedProcessor.CreateComponent(resource);

            await ReplyAsync(embeds: embeds, components: component);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerWeeklyPollOptionPresetCommands.cs AddWeeklyPollOptionPreset", ex);
        }
    }

    [Command("weekly poll preset edit")]
    [Alias(["wpp e", "weekly poll preset e", "wpp edit", "weeklypollpreset e", "weeklypollpreset edit"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Editing an existing Weekly Poll Option Preset")]
    public async Task EditWeeklyPollOptionPreset([Remainder][Name("name")] string presetName)
    {
        try
        {
            WeeklyPollOptionPresetResource resource = await weeklyPollOptionPresetService.GetPresetByNameAsync(presetName);

            Embed[] embeds = PollPresetEditEmbedProcessor.CreateEmbed(resource, true);
            MessageComponent component = PollPresetEditEmbedProcessor.CreateComponent(resource);

            await ReplyAsync(embeds: embeds, components: component);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerWeeklyPollOptionPresetCommands.cs EditWeeklyPollOptionPreset", ex);
        }
    }

    [Command("weekly poll preset remove")]
    [Alias(["wpp r", "weekly poll preset r", "wpp remove", "weeklypollpreset r", "weeklypollpreset remove"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Removing an existing Weekly Poll Option Preset")]
    public async Task RemoveWeeklyPollOptionPreset([Remainder][Name("name")] string presetName)
    {
        try
        {
            DbProcessResultEnum result = await weeklyPollOptionPresetService.RemovePresetByNameAsync(presetName);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"The '{presetName}' preset has been removed.",
                DbProcessResultEnum.NotFound => "Preset does not exist.",
                _ => "Preset could not be removed!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerWeeklyPollOptionPresetCommands.cs RemoveWeeklyPollOptionPreset", ex);
        }
    }

    [Command("weekly poll preset list")]
    [Alias(["wpp l", "weekly poll preset l", "wpp list", "weeklypollpreset l", "weeklypollpreset list"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Listing the Weekly Poll Option Presets that currently exist")]
    public async Task ListWeeklyPollOptionPreset()
    {
        try
        {
            List<WeeklyPollOptionPresetResource> weeklyPollResources = await weeklyPollOptionPresetService.GetPresetsAsync();

            Embed[] embed = PollPresetListEmbedProcessor.CreateEmbed(weeklyPollResources);

            await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerWeeklyPollOptionPresetCommands.cs ListWeeklyPollOptionPreset", ex);
        }
    }
}
