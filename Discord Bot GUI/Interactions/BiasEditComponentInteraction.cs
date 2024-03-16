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
        private readonly IIdolGroupService idolGroupService;
        private readonly IIdolImageService idolImageService;

        private Dictionary<string, Func<string, string, Task>> Actions { get; } = [];

        public BiasEditComponentInteraction(IIdolService idolService, IIdolGroupService idolGroupService, IIdolImageService idolImageService, Logging logger, Config config) : base(logger, config)
        {
            Actions.Add(BiasEditActionTypeEnum.EditIdol, CreateEditIdolModalAsync);
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

        private async Task CreateEditIdolModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);
            
            ModalBuilder mb = new ModalBuilder()
                .WithTitle("Edit Idol")
                .WithCustomId("EditIdolModal")
                .AddTextInput("Profile URL", "profileurl", placeholder: resource.ProfileUrl, maxLength: 200, required: true)
                .AddTextInput("Name", "name", placeholder: resource.Name, maxLength: 100, required: true)
                .AddTextInput("Group", "group", placeholder: resource.GroupName, maxLength: 100, required: true)
                .AddTextInput("Date Of Birth", "dateofbirth", placeholder: resource.DateOfBirth.ToString())
                .AddTextInput("Gender", "gender", placeholder: resource.Gender, maxLength: 10);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        private async Task CreateEditIdolExtendedModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);

            ModalBuilder mb = new ModalBuilder()
                .WithTitle("Edit Idol Extended")
                .WithCustomId("EditIdolExtendedModal")
                .AddTextInput("Stage Name", "stagename", placeholder: resource.StageName, maxLength: 100)
                .AddTextInput("Full Name", "fullname", placeholder: resource.FullName, maxLength: 100)
                .AddTextInput("Korean Stage Name", "koreanstagename", placeholder: resource.KoreanStageName, maxLength: 100)
                .AddTextInput("Korean Full name", "koreanfullname", placeholder: resource.KoreanFullName, maxLength: 100)
                .AddTextInput("Debut Date", "debutdate", placeholder: resource.DebutDate.ToString());

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        private async Task CreateEditGroupModalAsync(string idol, string group)
        {
            IdolGroupExtendedResource resource = await idolGroupService.GetIdolGroupDetailsAsync(group);

            ModalBuilder mb = new ModalBuilder()
                .WithTitle("Edit Group")
                .WithCustomId("EditGroupModal")
                .AddTextInput("Name", "name", placeholder: resource.Name, maxLength: 100)
                .AddTextInput("Full Name", "fullname", placeholder: resource.FullName, maxLength: 100)
                .AddTextInput("Full Korean Name", "fullkoreanname", placeholder: resource.FullKoreanName, maxLength: 100)
                .AddTextInput("Debut Date", "debutdate", placeholder: resource.DebutDate.ToString(), maxLength: 100);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        private async Task CreateChangeProfileLinkModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);

            ModalBuilder mb = new ModalBuilder()
                .WithTitle("Change Idol Profile Link")
                .WithCustomId("ChangeIdolProfileLinkModal")
                .AddTextInput("Profile URL", "profileurl", placeholder: resource.ProfileUrl, maxLength: 200, required: true);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        private async Task CreateChangeGroupModalAsync(string idol, string group)
        {
            IdolExtendedResource resource = await idolService.GetIdolDetailsAsync(idol, group);

            ModalBuilder mb = new ModalBuilder()
                .WithTitle("Change Idol Group")
                .WithCustomId("ChangeIdolGroupModal")
                .AddTextInput("Group", "group", placeholder: resource.GroupName, maxLength: 100, required: true);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        private async Task RemoveImageAsync(string idol, string group)
        {
            DbProcessResultEnum result = await idolImageService.RemoveIdolImagesAsync(idol, group);

            if(result == DbProcessResultEnum.Success)
            {
                await ReplyAsync("Images removed successfully!");
            }
            else if(result == DbProcessResultEnum.NotFound)
            {
                await ReplyAsync("Idol could not be found.");
            }
            else
            {
                await ReplyAsync("Images could not be removed!");
            }
        }
    }
}
