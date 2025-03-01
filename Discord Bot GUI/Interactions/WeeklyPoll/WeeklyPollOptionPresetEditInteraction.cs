using Discord_Bot.Core.Configuration;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Discord;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Database.DBServices;
using Discord_Bot.Enums;
using Discord_Bot.Processors.EmbedProcessors.Polls;
using Discord_Bot.Resources;

namespace Discord_Bot.Interactions.WeeklyPoll
{
    public class WeeklyPollOptionPresetEditInteraction(
    IWeeklyPollOptionPresetService weeklyPollOptionPresetService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseInteraction(serverService, logger, config)
    {
        private readonly IWeeklyPollOptionPresetService weeklyPollOptionPresetService = weeklyPollOptionPresetService;

        [ComponentInteraction("EditPollPreset_*")]
        [RequireUserPermission(ChannelPermission.SendPolls)]
        [RequireContext(ContextType.Guild)]
        public async Task EditWeeklyPollPresetHandler(int presetId)
        {
            try
            {
                WeeklyPollOptionPresetResource resource = await weeklyPollOptionPresetService.GetPresetByIdAsync(presetId);

                void Modify(ModalBuilder builder) => builder
                    .UpdateTextInput("name", string.IsNullOrEmpty(resource.Name) ? null : resource.Name)
                    .UpdateTextInput("description", string.IsNullOrEmpty(resource.Description) ? null : resource.Description);

                await RespondWithModalAsync<EditWeeklyPollOptionPresetModal>($"EditPollPresetModal_{resource.WeeklyPollOptionPresetId}", modifyModal: Modify);
                return;
            }
            catch (Exception ex)
            {
                logger.Error("WeeklyPollOptionPresetEditInteraction.cs EditWeeklyPollPresetHandler", ex);
            }

            await RespondAsync("Something went wrong during the process.");
        }

        [ModalInteraction("EditPollPresetModal_*")]
        [RequireUserPermission(ChannelPermission.SendPolls)]
        [RequireContext(ContextType.Guild)]
        public async Task EditWeeklyPollPresetModalSubmit(int presetId, EditWeeklyPollOptionPresetModal modal)
        {
            try
            {
                await DeferAsync();
                logger.Log($"Edit Poll Modal Submitted for poll with ID {presetId}", LogOnly: true);

                DbProcessResultEnum result = await weeklyPollOptionPresetService.UpdateAsync(presetId, modal);
                if (result == DbProcessResultEnum.Success)
                {
                    WeeklyPollOptionPresetResource resource = await weeklyPollOptionPresetService.GetPresetByIdAsync(presetId);

                    Embed[] embeds = PollPresetEditEmbedProcessor.CreateEmbed(resource, true);

                    await ModifyOriginalResponseAsync(x => x.Embeds = embeds);

                    await FollowupAsync("Edited poll successfully!", ephemeral: true);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("WeeklyPollOptionPresetEditInteraction.cs EditWeeklyPollPresetModalSubmit", ex);
            }
            await FollowupAsync("Something went wrong during the process.", ephemeral: true);
        }

        [ComponentInteraction("PollPreset_Change_*_*_*")]
        [RequireUserPermission(ChannelPermission.SendPolls)]
        [RequireContext(ContextType.Guild)]
        public async Task DynamicChangeWeeklyPollPresetButtonHandler(string fieldName, int presetId, bool? value)
        {
            try
            {
                await DeferAsync();
                DbProcessResultEnum result = await weeklyPollOptionPresetService.UpdateFieldAsync(presetId, fieldName, (!value).ToString());

                if (result == DbProcessResultEnum.Success)
                {
                    WeeklyPollOptionPresetResource resource = await weeklyPollOptionPresetService.GetPresetByIdAsync(presetId);

                    MessageComponent component = PollPresetEditEmbedProcessor.CreateComponent(resource);

                    await ModifyOriginalResponseAsync(x => x.Components = component);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("WeeklyPollOptionPresetEditInteraction.cs DynamicChangeWeeklyPollPresetButtonHandler", ex);
            }
            await FollowupAsync("Something went wrong during the process.", ephemeral: true);
        }
    }
}
