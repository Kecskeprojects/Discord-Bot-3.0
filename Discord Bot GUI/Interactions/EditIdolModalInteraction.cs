using Discord.Interactions;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions
{
    public class EditIdolModalInteraction(IIdolService idolService, IIdolGroupService idolGroupService, Logging logger, Config config) : BaseInteraction(logger, config)
    {
        private readonly IIdolService idolService = idolService;
        private readonly IIdolGroupService idolGroupService = idolGroupService;

        //Todo:
        //The modal items can be handed over to the DB services to edit the entities, together with the identifying IDs
        //If EditIdolModalSubmit recieves an entirely new group's name not found in the database, an entirely new group will be created
        //If the group of the idol is reassigned, the idol's extended data should be removed
        [ModalInteraction("EditIdolModal_*")]
        [RequireOwner]
        public async Task EditIdolModalSubmit(int idolId, EditIdolModal modal)
        {
            try
            {
                logger.Log($"Edit Idol Modal Submitted for idol with ID {idolId}", LogOnly: true);

                DbProcessResultEnum result = await idolService.UpdateAsync(idolId, modal);
                if (result == DbProcessResultEnum.Success)
                {
                    await RespondAsync("Edited bias successfully!");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("EditIdolModalInteraction.cs EditIdolModalSubmit", ex.ToString());
            }
            await RespondAsync("Bias could not be edited!");
        }

        [ModalInteraction("EditIdolExtendedModal_*")]
        public async Task EditIdolExtendedModalSubmit(int idolId, EditIdolExtendedModal modal)
        {
            try
            {
                logger.Log($"Edit Idol Extended Modal Submitted for idol with ID {idolId}", LogOnly: true);

                DbProcessResultEnum result = await idolService.UpdateAsync(idolId, modal);
                if (result == DbProcessResultEnum.Success)
                {
                    await RespondAsync("Edited bias successfully!");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("EditIdolModalInteraction.cs EditIdolExtendedModalSubmit", ex.ToString());
            }
            await RespondAsync("Bias could not be edited!");
        }

        [ModalInteraction("EditGroupModal_*")]
        [RequireOwner]
        public async Task EditGroupModalSubmit(int groupId, EditGroupModal modal)
        {
            try
            {
                logger.Log($"Edit Group Modal Submitted for group with ID {groupId}", LogOnly: true);

                DbProcessResultEnum result = await idolGroupService.UpdateAsync(groupId, modal);
                if (result == DbProcessResultEnum.Success)
                {
                    await RespondAsync("Edited group successfully!");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("EditIdolModalInteraction.cs EditGroupModalSubmit", ex.ToString());
            }
            await RespondAsync("Group could not be edited!");
        }

        [ModalInteraction("ChangeIdolProfileLinkModal_*")]
        [RequireOwner]
        public async Task ChangeIdolProfileLinkModalSubmit(int idolId, ChangeIdolProfileLinkModal modal)
        {
            try
            {
                logger.Log($"Change Idol Profile Link Modal Submitted for idol with ID {idolId}", LogOnly: true);

                DbProcessResultEnum result = await idolService.UpdateAsync(idolId, modal);
                if (result == DbProcessResultEnum.Success)
                {
                    await RespondAsync("Edited bias successfully!");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("EditIdolModalInteraction.cs ChangeIdolProfileLinkModalSubmit", ex.ToString());
            }
            await RespondAsync("Bias could not be edited!");
        }

        [ModalInteraction("ChangeIdolGroupModal_*")]
        [RequireOwner]
        public async Task ChangeIdolGroupModalSubmit(int idolId, ChangeIdolGroupModal modal)
        {
            try
            {
                logger.Log($"Change Idol Group Modal Submitted for idol with ID {idolId}", LogOnly: true);

                DbProcessResultEnum result = await idolService.UpdateAsync(idolId, modal);
                if (result == DbProcessResultEnum.Success)
                {
                    await RespondAsync("Edited bias successfully!");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("EditIdolModalInteraction.cs ChangeIdolGroupModalSubmit", ex.ToString());
            }
            await RespondAsync("Bias could not be edited!");
        }
    }
}
