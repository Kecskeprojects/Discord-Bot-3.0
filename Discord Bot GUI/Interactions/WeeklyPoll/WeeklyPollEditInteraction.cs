using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Polls;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions.WeeklyPoll;

public class WeeklyPollEditInteraction(
    IWeeklyPollOptionPresetService weeklyPollOptionPresetService,
    IWeeklyPollService weeklyPollService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseInteraction(serverService, logger, config)
{
    private readonly IWeeklyPollOptionPresetService weeklyPollOptionPresetService = weeklyPollOptionPresetService;
    private readonly IWeeklyPollService weeklyPollService = weeklyPollService;

    [ComponentInteraction("EditPoll_*")]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    public async Task EditWeeklyPollHandler(int pollId)
    {
        try
        {
            WeeklyPollResource resource = await weeklyPollService.GetPollByIdAsync(pollId);

            RestRole role = resource.RoleDiscordId.HasValue
                ? await Context.Guild.GetRoleAsync(resource.RoleDiscordId.Value)
                : null;
            SocketGuildChannel channel = resource.ChannelDiscordId.HasValue
                ? Context.Guild.GetChannel(resource.ChannelDiscordId.Value)
                : null;

            void Modify(ModalBuilder builder) => builder
                .UpdateTextInput("name", string.IsNullOrEmpty(resource.Name) ? null : resource.Name)
                .UpdateTextInput("polltitle", string.IsNullOrEmpty(resource.Title) ? null : resource.Title)
                .UpdateTextInput("channel", channel?.Name)
                .UpdateTextInput("role", role?.Name);

            await RespondWithModalAsync<EditWeeklyPollModal>($"EditPollModal_{resource.WeeklyPollId}", modifyModal: Modify);
            return;
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollEditInteraction.cs EditWeeklyPollHandler", ex);
        }

        await RespondAsync("Something went wrong during the process.");
    }

    [ModalInteraction("EditPollModal_*")]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    public async Task EditWeeklyPollModalSubmit(int pollId, EditWeeklyPollModal modal)
    {
        try
        {
            await DeferAsync();
            logger.Log($"Edit Poll Modal Submitted for poll with ID {pollId}", LogOnly: true);

            SocketGuildChannel channel = Context.Guild.Channels.FirstOrDefault(x => x.Name.Equals(modal.Channel.Trim(), StringComparison.OrdinalIgnoreCase));
            if (channel == null)
            {
                await RespondAsync("Channel not found!");
                return;
            }
            SocketRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name.Equals(modal.Role.Trim(), StringComparison.OrdinalIgnoreCase));

            DbProcessResultEnum result = await weeklyPollService.UpdateAsync(pollId, modal, channel.Id, role?.Id, role?.Name);
            if (result == DbProcessResultEnum.Success)
            {
                WeeklyPollResource resource = await weeklyPollService.GetPollByIdAsync(pollId);

                Embed[] embeds = PollEditEmbedProcessor.CreateEmbed(resource, true);

                _ = await ModifyOriginalResponseAsync(x => x.Embeds = embeds);

                _ = await FollowupAsync("Edited poll successfully!", ephemeral: true);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollEditInteraction.cs EditWeeklyPollModalSubmit", ex);
        }
        _ = await FollowupAsync("Something went wrong during the process.", ephemeral: true);
    }

    [ComponentInteraction("Poll_Change_*_*_*")]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    public async Task DynamicChangeWeeklyPollButtonHandler(string fieldName, int pollId, bool? value)
    {
        try
        {
            await DeferAsync();
            DbProcessResultEnum result = await weeklyPollService.UpdateFieldAsync(pollId, fieldName, (!value).ToString());

            if (result == DbProcessResultEnum.Success)
            {
                WeeklyPollEditResource resource = await weeklyPollService.GetPollByIdForEditAsync(pollId);
                List<WeeklyPollOptionPresetResource> presets = await weeklyPollOptionPresetService.GetActivePresetsAsync();

                MessageComponent component = PollEditEmbedProcessor.CreateComponent(resource, presets);

                _ = await ModifyOriginalResponseAsync(x => x.Components = component);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollEditInteraction.cs DynamicChangeWeeklyPollButtonHandler", ex);
        }
        _ = await FollowupAsync("Something went wrong during the process.", ephemeral: true);
    }

    [ComponentInteraction("Poll_SelectChange_*_*")]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    public async Task DynamicChangeWeeklyPollSelectHandler(string fieldName, int pollId, string[] selectedValues)
    {
        try
        {
            await DeferAsync();
            DbProcessResultEnum result = await weeklyPollService.UpdateFieldAsync(pollId, fieldName, selectedValues[0]);

            if (result == DbProcessResultEnum.Success)
            {
                WeeklyPollEditResource resource = await weeklyPollService.GetPollByIdForEditAsync(pollId);
                List<WeeklyPollOptionPresetResource> presets = await weeklyPollOptionPresetService.GetActivePresetsAsync();

                MessageComponent embeds = PollEditEmbedProcessor.CreateComponent(resource, presets);

                _ = await ModifyOriginalResponseAsync(x => x.Components = embeds);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollEditInteraction.cs DynamicChangeWeeklyPollSelectHandler", ex);
        }
        _ = await FollowupAsync("Something went wrong during the process.", ephemeral: true);
    }
}
