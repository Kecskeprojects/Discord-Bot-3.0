using Discord;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Processors.EmbedProcessors.Polls;
public class PollEditEmbedProcessor
{
    public static Embed[] CreateEmbed(WeeklyPollEditResource poll, bool isEdit)
    {
        EmbedBuilder builder = new();
        builder.WithTitle($"{(isEdit ? $"Edit '{poll.Name}'" : "Create new Weekly Poll")}");

        builder.AddField("Edit Button", "Name of Poll, Title of Poll, Channel the poll is sent to and Role that can be optionally set");

        builder.AddField("Other Buttons", "They describe the current state of what they edit\nClicking it will switch back and forth between it's two states");

        builder.AddField("First 3 Select Fields", "They describe the current state of what they edit\nClicking it will switch back and forth between it's two states");

        builder.AddField("4th Select Field", "If visible, selecting an option allows the creation/editing of an answer\nIf an option is empty, it is a potential answer that can still be defined");

        builder.WithColor(Color.DarkBlue);
        return [builder.Build()];
    }

    public static MessageComponent CreateComponent(WeeklyPollEditResource poll, List<WeeklyPollOptionPresetResource> presets)
    {
        ActionRowBuilder buttonRow = new();
        buttonRow
            .WithButton(label: "Edit Poll"
                            , customId: $"EditPoll_{poll.WeeklyPollId}"
                            , style: ButtonStyle.Primary)
            .WithButton(label: poll.IsMultipleAnswer ? "Multi Answer" : "Single Answer"
                            , customId: $"Poll_Change_IsMultiAnswer_{poll.WeeklyPollId}_{poll.IsMultipleAnswer}"
                            , style: ButtonStyle.Primary)
            .WithButton(label: poll.IsActive ? "Active" : "Inactive"
                            , customId: $"Poll_Change_IsActive_{poll.WeeklyPollId}_{poll.IsActive}"
                            , style: poll.IsActive ? ButtonStyle.Success : ButtonStyle.Danger);

        ActionRowBuilder closePollInRow = CreateClosePollInRow(poll);
        ActionRowBuilder dayOfWeekRow = CreateDayOfWeekRow(poll);
        ActionRowBuilder optionPresetRow = CreateOptionPresetRow(poll, presets);

        ComponentBuilder components = new();
        components
            .AddRow(buttonRow)
            .AddRow(closePollInRow)
            .AddRow(dayOfWeekRow)
            .AddRow(optionPresetRow);

        if (!poll.OptionPresetId.HasValue)
        {
            ActionRowBuilder customOptionRow = CreateCustomOptionRow(poll);
            components.AddRow(customOptionRow);
        }

        return components.Build();
    }

    private static ActionRowBuilder CreateDayOfWeekRow(WeeklyPollEditResource poll)
    {
        SelectMenuBuilder dayOfWeekSelect = new()
        {
            CustomId = $"Poll_Change_DayOfWeek_{poll.WeeklyPollId}",
            Placeholder = $"Select when poll is sent weekly",
            MinValues = 1,
            MaxValues = 1,
            Type = ComponentType.SelectMenu
        };

        Enum.GetNames<DayOfWeek>()
            .ToList()
            .ForEach(y => dayOfWeekSelect.AddOption(
                            y.ToString(),
                            y.ToString(),
                            isDefault: poll.RepeatOnDayOfWeek == y.ToString()));

        ActionRowBuilder dayOfWeekRow = new();
        dayOfWeekRow.WithSelectMenu(dayOfWeekSelect);
        return dayOfWeekRow;
    }

    private static ActionRowBuilder CreateClosePollInRow(WeeklyPollEditResource poll)
    {
        SelectMenuBuilder closePollInSelect = new()
        {
            CustomId = $"Poll_Change_CloseInTimeSpanTicks_{poll.WeeklyPollId}",
            Placeholder = $"Select how long until poll is closed",
            MinValues = 1,
            MaxValues = 1,
            Type = ComponentType.SelectMenu
        };
        Enum.GetValues<PollCloseInEnum>()
            .ToList()
            .ForEach(y =>
                closePollInSelect
                    .AddOption(
                        y.ToFriendlyString(),
                        y.ConvertToTimeSpanTicks().ToString(),
                        isDefault: y.ConvertToTimeSpanTicks() == poll.CloseInTimeSpanTicks));

        ActionRowBuilder closePollInRow = new();
        closePollInRow.WithSelectMenu(closePollInSelect);
        return closePollInRow;
    }

    private static ActionRowBuilder CreateOptionPresetRow(WeeklyPollEditResource poll, List<WeeklyPollOptionPresetResource> presets)
    {
        SelectMenuBuilder optionPresetSelect = new()
        {
            CustomId = $"Poll_Change_OptionPreset_{poll.WeeklyPollId}",
            Placeholder = $"Select which preset to use for answers",
            MinValues = 1,
            MaxValues = 1,
            Type = ComponentType.SelectMenu
        };

        optionPresetSelect.AddOption(
            "Custom Options"
            , "custom"
            , "You will be able to define the answers yourself"
            , isDefault: poll.OptionPresetId == null);

        foreach (WeeklyPollOptionPresetResource preset in presets)
        {
            optionPresetSelect.AddOption(
                preset.Name
                , null
                , preset.Description
                , isDefault: poll.OptionPresetId == preset.OptionPresetId);
        }

        ActionRowBuilder optionPresetRow = new();
        optionPresetRow.WithSelectMenu(optionPresetSelect);
        return optionPresetRow;
    }

    private static ActionRowBuilder CreateCustomOptionRow(WeeklyPollEditResource poll)
    {
        SelectMenuBuilder optionPresetSelect = new()
        {
            CustomId = $"Poll_Change_CustomOption_{poll.WeeklyPollId}",
            Placeholder = $"Select an option to edit it",
            MinValues = 1,
            MaxValues = 1,
            Type = ComponentType.SelectMenu
        };

        //Values can be separated by the _ character so that we have both an order number and an ID as values
        for (int i = 0; i < 10; i++)
        {
            if (poll.Options.Count > i)
            {
                optionPresetSelect.AddOption($"{i + 1}# {poll.Options[i].Title}", $"{i}_{poll.Options[i].WeeklyPollOptionId}");
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
