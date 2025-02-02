using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using System;

namespace Discord_Bot.Commands.Owner;

//[Name("Weekly Poll Option Preset")]
//[Remarks("Owner")]
//[Summary("Manage presets for Weekly Polls")]
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
    public void AddWeeklyPollOptionPreset([Name("user name")] IUser user)
    {
        try
        {
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollOptionPresetCommands.cs AddWeeklyPollOptionPreset", ex);
        }
    }

    [Command("weekly poll preset edit")]
    [Alias(["wpp e", "weekly poll preset e", "wpp edit", "weeklypollpreset e", "weeklypollpreset edit"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Editing an existing Weekly Poll Option Preset")]
    public void EditWeeklyPollOptionPreset([Name("user name")] IUser user)
    {
        try
        {
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollOptionPresetCommands.cs EditWeeklyPollOptionPreset", ex);
        }
    }

    [Command("weekly poll preset remove")]
    [Alias(["wpp r", "weekly poll preset r", "wpp remove", "weeklypollpreset r", "weeklypollpreset remove"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Removing an existing Weekly Poll Option Preset")]
    public void RemoveWeeklyPollOptionPreset([Name("user name")] IUser user)
    {
        try
        {
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollOptionPresetCommands.cs RemoveWeeklyPollOptionPreset", ex);
        }
    }

    [Command("weekly poll preset list")]
    [Alias(["wpp l", "weekly poll preset l", "wpp list", "weeklypollpreset l", "weeklypollpreset list"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Listing the Weekly Poll Option Presets that currently exist")]
    public void ListWeeklyPollOptionPreset([Name("user name")] IUser user)
    {
        try
        {
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollOptionPresetCommands.cs ListWeeklyPollOptionPreset", ex);
        }
    }
}
