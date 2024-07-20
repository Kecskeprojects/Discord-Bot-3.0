using Discord;
using Discord_Bot.Enums;

namespace Discord_Bot.Processors.MessageProcessor;
public static class EditBiasDataMessageProcessor
{
    public static MessageComponent CreateComponent(string biasName, string biasGroup)
    {
        SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
                            .WithPlaceholder("Select edit type")
                            .WithCustomId("EditIdolData")
                            .AddOption("Edit Group", $"{(int) BiasEditActionTypeEnum.EditGroup};{biasName};{biasGroup}", "Edit group currently related to idol")
                            .AddOption("Edit Idol", $"{(int) BiasEditActionTypeEnum.EditIdol};{biasName};{biasGroup}", "Edit idol in question")
                            .AddOption("Edit Idol Extended", $"{(int) BiasEditActionTypeEnum.EditIdolExtended};{biasName};{biasGroup}", "Edit idol's extra details")
                            .AddOption("Change Group", $"{(int) BiasEditActionTypeEnum.ChangeGroup};{biasName};{biasGroup}", "Change the group the idol belongs to")
                            .AddOption("Change Profile Link", $"{(int) BiasEditActionTypeEnum.ChangeProfileLink};{biasName};{biasGroup}", "Edit idol's profile page link")
                            .AddOption("Override Image", $"{(int) BiasEditActionTypeEnum.OverrideImage};{biasName};{biasGroup}", "Replaces currently used image")
                            .AddOption("Remove Images", $"{(int) BiasEditActionTypeEnum.RemoveImage};{biasName};{biasGroup}", "Remove all images stored for idol");

        ComponentBuilder builder = new();
        builder.WithSelectMenu(menuBuilder);
        return builder.Build();
    }
}
