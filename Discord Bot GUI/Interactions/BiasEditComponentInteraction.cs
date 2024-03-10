using Discord;
using Discord.Interactions;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions
{
    public class BiasEditComponentInteraction : BaseInteraction
    {
        private readonly IIdolService idolService;

        private Dictionary<string, Func<string, string, Task>> Actions { get; } = [];

        public BiasEditComponentInteraction(IIdolService idolService, Logging logger, Config config) : base(logger, config)
        {
            Actions.Add(BiasEditActionTypeEnum.EditIdol, CreateEditIdolModalAsync);
            Actions.Add(BiasEditActionTypeEnum.EditGroup, CreateEditGroupModalAsync);
            Actions.Add(BiasEditActionTypeEnum.ChangeProfileLink, CreateChangeProfileLinkModalAsync);
            Actions.Add(BiasEditActionTypeEnum.ChangeGroup, CreateChangeGroupModalAsync);
            Actions.Add(BiasEditActionTypeEnum.RemoveImage, RemoveImageAsync);
            this.idolService = idolService;
        }

        [ComponentInteraction("editIdolData")]
        public async Task EditIdolMenuHandler(string[] selectedAction)
        {
            try
            {
                logger.Log($"Idol menu item selected with following parameters: {string.Join(",", selectedAction)}", LogOnly: true);

                BiasEditData data = new(selectedAction[0]);

                if (Actions.TryGetValue(data.Action, out Func<string, string, Task> value))
                {
                    //To streamline the process we use a dictionary instead of a switch
                    await value.Invoke(data.BiasName, data.BiasGroupName);
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
        private async Task CreateEditIdolModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);

            ModalBuilder mb = new ModalBuilder()
                .WithTitle("Edit Idol")
                .WithCustomId("editIdolModal")
                .AddTextInput("Profile URL", "profileurl", placeholder: resource.ProfileUrl, maxLength: 200, required: true)
                .AddTextInput("Name", "name", placeholder: resource.Name, maxLength: 100, required: true)
                .AddTextInput("Group", "group", placeholder: resource.GroupName, maxLength: 100, required: true)
                .AddTextInput("Stage Name", "stagename", placeholder: resource.StageName, maxLength: 100)
                .AddTextInput("Full Name", "fullname", placeholder: resource.StageName, maxLength: 100)
                .AddTextInput("Korean Stage Name", "koreanstagename", placeholder: resource.KoreanStageName, maxLength: 100)
                .AddTextInput("Korean Full name", "koreanfullname", placeholder: resource.KoreanFullName, maxLength: 100)
                .AddTextInput("Date Of Birth", "dateofbirth", placeholder: resource.DateOfBirth.ToString())
                .AddTextInput("Gender", "gender", placeholder: resource.Gender, maxLength: 10);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        private async Task CreateEditGroupModalAsync(string idol, string group)
        {
            throw new NotImplementedException();
        }

        private async Task CreateChangeProfileLinkModalAsync(string idol, string group)
        {
            throw new NotImplementedException();
        }

        private async Task CreateChangeGroupModalAsync(string idol, string group)
        {
            throw new NotImplementedException();
        }

        private async Task RemoveImageAsync(string idol, string group)
        {
            throw new NotImplementedException();
        }
    }
}
