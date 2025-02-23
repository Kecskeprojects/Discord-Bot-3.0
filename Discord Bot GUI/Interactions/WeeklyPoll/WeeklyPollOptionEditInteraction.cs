using Discord;
using Discord.Interactions;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Polls;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions.WeeklyPoll;

public class WeeklyPollOptionEditInteraction(
IWeeklyPollOptionPresetService weeklyPollOptionPresetService,
IWeeklyPollOptionService weeklyPollOptionService,
IWeeklyPollService weeklyPollService,
IServerService serverService,
BotLogger logger,
Config config) : BaseInteraction(serverService, logger, config)
{
    private readonly IWeeklyPollOptionPresetService weeklyPollOptionPresetService = weeklyPollOptionPresetService;
    private readonly IWeeklyPollOptionService weeklyPollOptionService = weeklyPollOptionService;
    private readonly IWeeklyPollService weeklyPollService = weeklyPollService;

    [ComponentInteraction("PollOption_Change_CustomOption_*")]
    [RequireUserPermission(ChannelPermission.SendPolls)]
    [RequireContext(ContextType.Guild)]
    public async Task EditWeeklyPollOptionHandler(int pollId, string[] selectedOption)
    {
        try
        {
            byte orderNumber = byte.Parse(selectedOption[0].Split("_")[0]);
            int optionId = int.Parse(selectedOption[0].Split("_")[1]);
            WeeklyPollOptionResource resource = await weeklyPollOptionService.GetOrCreateOptionAsync(pollId, optionId, orderNumber);

            void Modify(ModalBuilder builder) => builder
                .UpdateTextInput("optiontitle", string.IsNullOrEmpty(resource.Title) ? null : resource.Title);

            await RespondWithModalAsync<EditWeeklyPollOptionModal>($"EditPollOptionModal_{resource.WeeklyPollId}_{resource.WeeklyPollOptionId}", modifyModal: Modify);
            return;
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionEditInteraction.cs EditWeeklyPollOptionHandler", ex);
        }
        await RespondAsync("Something went wrong during the process.");
    }

    [ModalInteraction("EditPollOptionModal_*_*")]
    [RequireOwner]
    public async Task EditWeeklyPollOptionModalSubmit(int pollId, int pollOptionId, EditWeeklyPollOptionModal modal)
    {
        try
        {
            await DeferAsync();
            logger.Log($"Edit Poll Option Modal Submitted for poll option with ID {pollOptionId}", LogOnly: true);

            DbProcessResultEnum result = await weeklyPollOptionService.UpdateAsync(pollOptionId, modal.OptionTitle);
            if (result == DbProcessResultEnum.Success)
            {
                WeeklyPollEditResource resource = await weeklyPollService.GetPollByIdForEditAsync(pollId);
                List<WeeklyPollOptionPresetResource> presets = await weeklyPollOptionPresetService.GetActivePresetsAsync();

                MessageComponent embeds = PollEditEmbedProcessor.CreateComponent(resource, presets);

                await ModifyOriginalResponseAsync(x => x.Components = embeds);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionEditInteraction.cs EditWeeklyPollOptionModalSubmit", ex);
        }
        await FollowupAsync("Something went wrong during the process.");
    }
}
