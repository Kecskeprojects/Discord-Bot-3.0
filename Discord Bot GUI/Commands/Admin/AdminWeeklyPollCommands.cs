using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Polls;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

[Name("Weekly Poll")]
[Remarks("Admin")]
[Summary("Manage Weekly Polls on your server")]
public class AdminWeeklyPollCommands(
    IWeeklyPollService weeklyPollService,
    IWeeklyPollOptionPresetService weeklyPollOptionPresetService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IWeeklyPollService weeklyPollService = weeklyPollService;

    [Command("weekly poll create")]
    [Alias(["wp c", "weekly poll c", "wp create", "weeklypoll c", "weeklypoll create"])]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    [Summary("Adding a new Weekly Poll to the server (calling the command will create a dummy poll unless there already an incomplete poll present)")]
    public async Task AddWeeklyPoll()
    {
        try
        {
            WeeklyPollEditResource resource = await weeklyPollService.GetOrCreateDummyPollAsync(Context.Guild.Id);
            List<WeeklyPollOptionPresetResource> presets = await weeklyPollOptionPresetService.GetActivePresetsAsync();

            Embed[] embeds = PollEditEmbedProcessor.CreateEmbed(resource, false);
            MessageComponent component = PollEditEmbedProcessor.CreateComponent(resource, presets);

            _ = await ReplyAsync(embeds: embeds, components: component);
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
    public async Task EditWeeklyPoll([Remainder][Name("name")] string pollName)
    {
        try
        {
            WeeklyPollEditResource resource = await weeklyPollService.GetPollByNameForEditAsync(Context.Guild.Id, pollName);
            List<WeeklyPollOptionPresetResource> presets = await weeklyPollOptionPresetService.GetActivePresetsAsync();

            Embed[] embeds = PollEditEmbedProcessor.CreateEmbed(resource, true);
            MessageComponent component = PollEditEmbedProcessor.CreateComponent(resource, presets);

            _ = await ReplyAsync(embeds: embeds, components: component);
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
    public async Task RemoveWeeklyPoll([Remainder][Name("name")] string pollName)
    {
        try
        {
            DbProcessResultEnum result = await weeklyPollService.RemovePollByNameAsync(Context.Guild.Id, pollName);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"The '{pollName}' poll has been removed.",
                DbProcessResultEnum.NotFound => "Poll does not exist.",
                _ => "Poll could not be removed!"
            };
            _ = await ReplyAsync(resultMessage);
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
    public async Task ListWeeklyPoll()
    {
        try
        {
            List<WeeklyPollResource> weeklyPollResources = await weeklyPollService.GetPollsByServerIdAsync(Context.Guild.Id);

            Embed[] embed = PollListEmbedProcessor.CreateEmbed(weeklyPollResources);

            _ = await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("AdminWeeklyPollCommands.cs ListWeeklyPoll", ex);
        }
    }
}
