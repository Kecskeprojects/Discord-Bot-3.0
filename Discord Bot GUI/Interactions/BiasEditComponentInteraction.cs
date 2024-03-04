using Discord;
using Discord.Interactions;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions
{
    public class BiasEditComponentInteraction(Logging logger, Config config) : BaseInteraction(logger, config)
    {
        private static Dictionary<BiasEditActionTypeEnum, Func<SocketInteractionContext, string, string, Task>> Actions {get; } = new()
        {
            { BiasEditActionTypeEnum.EditIdol, CreateEditIdolModalAsync },
            { BiasEditActionTypeEnum.EditGroup, CreateEditGroupModalAsync },
            { BiasEditActionTypeEnum.ChangeProfileLink, CreateChangeProfileLinkModalAsync },
            { BiasEditActionTypeEnum.ChangeGroup, CreateChangeGroupModalAsync },
            { BiasEditActionTypeEnum.RemoveImage, RemoveImageAsync }
        };

        [ComponentInteraction("editIdolData")]
        public async Task EditIdolMenuHandler(string[] selectedAction)
        {
            try
            {
                logger.Log($"Idol menu item selected with following parameters: {string.Join(",", selectedAction)}", LogOnly: true);

                BiasEditData data = new(selectedAction[0]);

                if (Actions.TryGetValue(data.Action, out Func<SocketInteractionContext, string, string, Task> value))
                {
                    //To streamline the process we use a dictionary instead of a switch
                    await value.Invoke(Context, data.BiasName, data.BiasGroupName);
                    return;
                }

                await Context.Interaction.RespondAsync("Something went wrong during the process.");
            }
            catch (Exception ex)
            {
                logger.Error("BiasEditComponentInteraction.cs EditIdolMenuHandler", ex.ToString());
            }
        }

        //Todo: Command to edit idol data manually, with an option to only change the profile link, command to change which group idol belongs to, command to edit group manually
        //The command could be one single command that goes like this: !edit idoldata [name]-[group] that will reply with a select
        //showing the following options: idoldata, groupdata, remove images, remove images will just remove all images associated with the idol
        //the other two will bring up modals to edit the idol's data, including which group they belong to, a new group name will result in an entirely new group item being created,
        //and another to edit the idol's current group's data, if the group is reassigned either way, all their extended data will be removed to get the entirely new data from the web
        //if possible the modal fields should show the current data, if any

        private static async Task CreateEditIdolModalAsync(SocketInteractionContext context, string idol, string group)
        {
            throw new NotImplementedException();
        }

        private static async Task CreateEditGroupModalAsync(SocketInteractionContext context, string idol, string group)
        {
            throw new NotImplementedException();
        }

        private static async Task CreateChangeProfileLinkModalAsync(SocketInteractionContext context, string idol, string group)
        {
            throw new NotImplementedException();
        }

        private static async Task CreateChangeGroupModalAsync(SocketInteractionContext context, string idol, string group)
        {
            throw new NotImplementedException();
        }

        private static async Task RemoveImageAsync(SocketInteractionContext context, string idol, string group)
        {
            throw new NotImplementedException();
        }
    }
}
