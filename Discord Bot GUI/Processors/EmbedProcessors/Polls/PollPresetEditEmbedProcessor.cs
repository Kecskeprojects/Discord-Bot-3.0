using Discord;
using Discord_Bot.Resources;
using System.Linq;

namespace Discord_Bot.Processors.EmbedProcessors.Polls;

public class PollPresetEditEmbedProcessor
{
    public static Embed[] CreateEmbed(WeeklyPollOptionPresetResource preset, bool isEdit)
    {
        EmbedBuilder builder = new();
        builder.WithTitle($"{(isEdit && !string.IsNullOrEmpty(preset.Name) ? $"Edit '{preset.Name}' Preset" : "Create new Weekly Poll Option Preset")}");

        builder.AddField("Edit Button", "Edit Name and Description of Preset");

        builder.AddField("Other Buttons", "They describe the current state of what they edit\nClicking it will switch back and forth between it's two states");

        builder.AddField("Select Field", "If visible, this field is to edit options tied to the Preset unless it's a special preset");

        builder.WithColor(Color.DarkBlue);
        return [builder.Build()];
    }

    public static MessageComponent CreateComponent(WeeklyPollOptionPresetResource preset)
    {
        ActionRowBuilder buttonRow = new();
        buttonRow
            .WithButton(label: "Edit Preset"
                            , customId: $"EditPollPreset_{preset.WeeklyPollOptionPresetId}"
                            , style: ButtonStyle.Primary)
            .WithButton(label: preset.IsSpecialPreset ? "Special Options" : "Normal Options"
                            , customId: $"PollPreset_Change_IsSpecialPreset_ {preset.WeeklyPollOptionPresetId}_{preset.IsSpecialPreset}"
                            , style: ButtonStyle.Primary)
            .WithButton(label: preset.IsActive ? "Active" : "Inactive"
                            , customId: $"PollPreset_Change_IsActive_{preset.WeeklyPollOptionPresetId}_{preset.IsActive}"
                            , style: preset.IsActive ? ButtonStyle.Success : ButtonStyle.Danger);

        ComponentBuilder components = new();
        components.AddRow(buttonRow);

        if (!preset.IsSpecialPreset)
        {
            ActionRowBuilder customOptionRow = CreateCustomOptionRow(preset);
            components.AddRow(customOptionRow);
        }

        return components.Build();
    }

    private static ActionRowBuilder CreateCustomOptionRow(WeeklyPollOptionPresetResource preset)
    {
        SelectMenuBuilder optionPresetSelect = new()
        {
            CustomId = $"PollOption_Change_CustomOption_{true}_{preset.WeeklyPollOptionPresetId}",
            Placeholder = $"Select an option to edit it",
            MinValues = 1,
            MaxValues = 1,
            Type = ComponentType.SelectMenu
        };

        //Values can be separated by the _ character so that we have both an order number and an ID as values
        for (int i = 0; i < 10; i++)
        {
            WeeklyPollOptionResource option = preset.Options.FirstOrDefault(x => x.OrderNumber == i);
            if (option != null)
            {
                optionPresetSelect.AddOption($"{i + 1}# {option.Title}", $"{i}_{option.WeeklyPollOptionPresetId}");
            }
            else
            {
                optionPresetSelect.AddOption($"{i + 1}#", $"{i}_0");
            }
        }

        ActionRowBuilder optionPresetRow = new();
        optionPresetRow.WithSelectMenu(optionPresetSelect);
        return optionPresetRow;
    }
}
