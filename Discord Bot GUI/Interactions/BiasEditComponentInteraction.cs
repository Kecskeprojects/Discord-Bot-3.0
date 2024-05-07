using Discord;
using Discord.Interactions;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
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
        private readonly IIdolGroupService idolGroupService;
        private readonly IIdolImageService idolImageService;

        private Dictionary<string, Func<string, string, Task>> Actions { get; } = [];

        public BiasEditComponentInteraction(IIdolService idolService, IIdolGroupService idolGroupService, IIdolImageService idolImageService, Logging logger, Config config) : base(logger, config)
        {
            Actions.Add(BiasEditActionTypeEnum.EditIdol, SendEditIdolModalAsync);
            Actions.Add(BiasEditActionTypeEnum.EditIdolExtended, CreateEditIdolExtendedModalAsync);
            Actions.Add(BiasEditActionTypeEnum.EditGroup, CreateEditGroupModalAsync);
            Actions.Add(BiasEditActionTypeEnum.ChangeProfileLink, CreateChangeProfileLinkModalAsync);
            Actions.Add(BiasEditActionTypeEnum.ChangeGroup, CreateChangeGroupModalAsync);
            Actions.Add(BiasEditActionTypeEnum.RemoveImage, RemoveImageAsync);
            this.idolService = idolService;
            this.idolGroupService = idolGroupService;
            this.idolImageService = idolImageService;
        }

        [ComponentInteraction("EditIdolData")]
        [RequireOwner]
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
            }
            catch (Exception ex)
            {
                logger.Error("BiasEditComponentInteraction.cs EditIdolMenuHandler", ex.ToString());
            }

            await RespondAsync("Something went wrong during the process.");
        }

        private async Task SendEditIdolModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);

            void Modify(ModalBuilder builder) => builder
                .UpdateTextInput("profileurl", resource.ProfileUrl)
                .UpdateTextInput("name", resource.Name)
                .UpdateTextInput("group", resource.GroupName)
                .UpdateTextInput("dateofbirth", resource.DateOfBirth.ToString())
                .UpdateTextInput("gender", resource.Gender);

            await RespondWithModalAsync<EditIdolModal>($"EditIdolModal_{resource.IdolId}", modifyModal: Modify);
        }

        private async Task CreateEditIdolExtendedModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);

            void Modify(ModalBuilder builder) => builder
                .UpdateTextInput("stagename", resource.StageName)
                .UpdateTextInput("fullname", resource.FullName)
                .UpdateTextInput("koreanstagename", resource.KoreanStageName)
                .UpdateTextInput("koreanfullname", resource.KoreanFullName)
                .UpdateTextInput("debutdate", resource.DebutDate.ToString());

            await RespondWithModalAsync<EditIdolExtendedModal>($"EditIdolExtendedModal_{resource.IdolId}", modifyModal: Modify);
        }

        private async Task CreateEditGroupModalAsync(string idol, string group)
        {
            IdolGroupExtendedResource resource = await idolGroupService.GetIdolGroupDetailsAsync(group);

            void Modify(ModalBuilder builder) => builder
                .UpdateTextInput("name", resource.Name)
                .UpdateTextInput("fullname", resource.FullName)
                .UpdateTextInput("fullkoreanname", resource.FullKoreanName)
                .UpdateTextInput("debutdate", resource.DebutDate.ToString());

            await RespondWithModalAsync<EditGroupModal>($"EditGroupModal_{resource.GroupId}", modifyModal: Modify);
        }

        private async Task CreateChangeProfileLinkModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);

            void Modify(ModalBuilder builder) => builder
                .UpdateTextInput("profileurl", resource.ProfileUrl);

            await RespondWithModalAsync<ChangeIdolProfileLinkModal>($"ChangeIdolProfileLinkModal_{resource.IdolId}", modifyModal: Modify);
        }

        private async Task CreateChangeGroupModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);

            void Modify(ModalBuilder builder) => builder
                .UpdateTextInput("group", resource.GroupName);

            await RespondWithModalAsync<ChangeIdolGroupModal>($"ChangeIdolGroupModal_{resource.IdolId}", modifyModal: Modify);
        }

        private async Task RemoveImageAsync(string idol, string group)
        {
            DbProcessResultEnum result = await idolImageService.RemoveIdolImagesAsync(idol, group);

            if (result == DbProcessResultEnum.Success)
            {
                await RespondAsync("Images removed successfully!");
            }
            else if (result == DbProcessResultEnum.NotFound)
            {
                await RespondAsync("Idol could not be found.");
            }
            else
            {
                await RespondAsync("Images could not be removed!");
            }
        }
    }
}
