using Discord;
using Discord_Bot.Enums;

namespace Discord_Bot.Processors.EmbedProcessors
{
    public class EditBiasDataEmbedProcessor
    {
        public static MessageComponent CreateComponent(string biasName, string biasGroup)
        {
            SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
                                .WithPlaceholder("Select edit type")
                                .WithCustomId("EditIdolData")
                                .AddOption("Edit Group", $"{BiasEditActionTypeEnum.EditGroup};{biasName};{biasGroup}", "Edit group currently related to idol")
                                .AddOption("Edit Idol", $"{BiasEditActionTypeEnum.EditIdol};{biasName};{biasGroup}", "Edit idol in question")
                                .AddOption("Edit Idol Extended", $"{BiasEditActionTypeEnum.EditIdolExtended};{biasName};{biasGroup}", "Edit idol's extra details")
                                .AddOption("Change Group", $"{BiasEditActionTypeEnum.ChangeGroup};{biasName};{biasGroup}", "Change the group the idol belongs to")
                                .AddOption("Change Profile Link", $"{BiasEditActionTypeEnum.ChangeProfileLink};{biasName};{biasGroup}", "Edit idol's profile page link")
                                .AddOption("Remove Images", $"{BiasEditActionTypeEnum.RemoveImage};{biasName};{biasGroup}", "Remove all images stored for idol");

            ComponentBuilder builder = new();
            builder.WithSelectMenu(menuBuilder);
            return builder.Build();
        }
    }
}
