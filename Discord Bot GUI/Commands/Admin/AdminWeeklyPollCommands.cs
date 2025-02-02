using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using System;

namespace Discord_Bot.Commands.Admin;

//[Name("Weekly Poll")]
//[Remarks("Admin")]
//[Summary("Manage Weekly Polls on your server")]
public class AdminWeeklyPollCommands(
    IWeeklyPollService weeklyPollService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IWeeklyPollService weeklyPollService = weeklyPollService;

    [Command("weekly poll create")]
    [Alias(["wp c", "weekly poll c", "wp create", "weeklypoll c", "weeklypoll create"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Adding a new Weekly Poll to the server")]
    public void AddWeeklyPoll([Name("user name")] IUser user)
    {
        try
        {
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollCommands.cs AddWeeklyPoll", ex);
        }
    }

    [Command("weekly poll edit")]
    [Alias(["wp e", "weekly poll e", "wp edit", "weeklypoll e", "weeklypoll edit"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Editing an existing Weekly Poll on the server")]
    public void EditWeeklyPoll([Name("user name")] IUser user)
    {
        try
        {
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollCommands.cs EditWeeklyPoll", ex);
        }
    }

    [Command("weekly poll remove")]
    [Alias(["wp r", "weekly poll r", "wp remove", "weeklypoll r", "weeklypoll remove"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Removing an existing Weekly Poll on the server")]
    public void RemoveWeeklyPoll([Name("user name")] IUser user)
    {
        try
        {
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollCommands.cs RemoveWeeklyPoll", ex);
        }
    }

    [Command("weekly poll list")]
    [Alias(["wp l", "weekly poll l", "wp list", "weeklypoll l", "weeklypoll list"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Listing the Weekly Polls currently active on the server")]
    public void ListWeeklyPoll([Name("user name")] IUser user)
    {
        try
        {
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollCommands.cs ListWeeklyPoll", ex);
        }
    }
}
