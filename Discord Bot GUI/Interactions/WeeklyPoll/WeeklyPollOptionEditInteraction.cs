using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Interactions;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Interfaces;
using Discord_Bot.Processors.EmbedProcessors.Polls;

namespace Discord_Bot.Interactions.WeeklyPoll;

public class WeeklyPollOptionEditInteraction(
IWeeklyPollOptionPresetService weeklyPollOptionPresetService,
IWeeklyPollOptionService weeklyPollOptionService,
IWeeklyPollService weeklyPollService,
IServerService serverService,
IBotLogger logger,
IConfig config) : BaseInteraction(serverService, logger, config)
{
    private readonly IWeeklyPollOptionPresetService weeklyPollOptionPresetService = weeklyPollOptionPresetService;
    private readonly IWeeklyPollOptionService weeklyPollOptionService = weeklyPollOptionService;
    private readonly IWeeklyPollService weeklyPollService = weeklyPollService;

    //A poll's answer menu (False) is for the server's administrators, a preset's (True) for the owner, as every server's polls
    //share the presets; until 2026-11-23 one handler took both, and "Create Polls" was enough
    [ComponentInteraction("PollOption_Change_CustomOption_False_*")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public Task EditWeeklyPollOptionHandler(int Id, string[] selectedOption)
    {
        return OpenOptionModalAsync(false, Id, selectedOption);
    }

    [ComponentInteraction("PollOption_Change_CustomOption_True_*")]
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    public Task EditWeeklyPollOptionPresetHandler(int Id, string[] selectedOption)
    {
        return OpenOptionModalAsync(true, Id, selectedOption);
    }

    private async Task OpenOptionModalAsync(bool isPresetOption, int Id, string[] selectedOption)
    {
        try
        {
            byte orderNumber = byte.Parse(selectedOption[0].Split("_")[0]);
            int optionId = int.Parse(selectedOption[0].Split("_")[1]);
            WeeklyPollOptionResource? resource = await weeklyPollOptionService.GetOrCreateOptionAsync(isPresetOption, Id, optionId, orderNumber);
            if (resource == null)
            {
                await RespondOrFollowupAsync("The poll option could not be found.", ephemeral: true);
                return;
            }

            void Modify(ModalBuilder builder) => builder
                .UpdateTextInput("optiontitle", string.IsNullOrEmpty(resource.Title) ? null : resource.Title);

            await RespondWithModalAsync<EditWeeklyPollOptionModal>($"EditPollOption{(isPresetOption ? "Preset" : "")}Modal_{Id}_{resource.WeeklyPollOptionId}", modifyModal: Modify);
            return;
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionEditInteraction.cs EditWeeklyPollOptionHandler", ex);
        }
        await RespondOrFollowupAsync("Something went wrong during the process.", ephemeral: true);
    }

    [ModalInteraction("EditPollOptionModal_*_*")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public async Task EditWeeklyPollOptionModalSubmit(int pollId, int pollOptionId, EditWeeklyPollOptionModal modal)
    {
        try
        {
            await DeferAsync();
            logger.Log($"Edit Poll Option Modal Submitted for poll option with ID {pollOptionId}", LogOnly: true);

            DbProcessResultEnum result = await weeklyPollOptionService.UpdateAsync(pollOptionId, modal.OptionTitle);
            if (result == DbProcessResultEnum.Success)
            {
                WeeklyPollEditResource? resource = await weeklyPollService.GetPollByIdForEditAsync(pollId);
                if (resource == null)
                {
                    await RespondOrFollowupAsync("The change was saved, but the poll could not be loaded again to refresh this message.", ephemeral: true);
                    return;
                }
                List<WeeklyPollOptionPresetResource> presets = await weeklyPollOptionPresetService.GetActivePresetsAsync();

                MessageComponent embeds = PollEditEmbedProcessor.CreateComponent(resource, presets);

                _ = await ModifyOriginalResponseAsync(x => x.Components = embeds);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionEditInteraction.cs EditWeeklyPollOptionModalSubmit", ex);
        }
        await RespondOrFollowupAsync("Something went wrong during the process.", ephemeral: true);
    }

    [ModalInteraction("EditPollOptionPresetModal_*_*")]
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    public async Task EditWeeklyPollOptionPresetModalSubmit(int presetId, int pollOptionId, EditWeeklyPollOptionModal modal)
    {
        try
        {
            await DeferAsync();
            logger.Log($"Edit Poll Option Modal Submitted for poll option with ID {pollOptionId}", LogOnly: true);

            DbProcessResultEnum result = await weeklyPollOptionService.UpdateAsync(pollOptionId, modal.OptionTitle);
            if (result == DbProcessResultEnum.Success)
            {
                WeeklyPollOptionPresetResource? resource = await weeklyPollOptionPresetService.GetPresetByIdAsync(presetId);
                if (resource == null)
                {
                    await RespondOrFollowupAsync("The change was saved, but the preset could not be loaded again to refresh this message.", ephemeral: true);
                    return;
                }

                MessageComponent embeds = PollPresetEditEmbedProcessor.CreateComponent(resource);

                _ = await ModifyOriginalResponseAsync(x => x.Components = embeds);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionEditInteraction.cs EditWeeklyPollOptionPresetModalSubmit", ex);
        }
        await RespondOrFollowupAsync("Something went wrong during the process.", ephemeral: true);
    }
}
