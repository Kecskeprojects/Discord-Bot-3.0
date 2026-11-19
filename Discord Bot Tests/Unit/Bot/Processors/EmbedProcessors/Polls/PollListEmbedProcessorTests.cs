using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Processors.EmbedProcessors.Polls;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors.Polls;

//!weekly poll list and !weekly poll preset list
public sealed class PollListEmbedProcessorTests
{
    [Fact]
    public void CreateEmbed_GivesEachPollItsKindAndAnswerCount()
    {
        List<WeeklyPollResource> polls =
        [
            new() { Name = "weekly", IsMultipleAnswer = true, Options = ["Yes", "No"] },
            new() { Name = "daily", Options = ["Up"] }
        ];

        Embed embed = Assert.Single(PollListEmbedProcessor.CreateEmbed(polls));

        Assert.Equal("Server Weekly Polls:", embed.Title);
        Assert.Equal(["weekly", "daily"], embed.Fields.Select(field => field.Name));
        Assert.Equal(["Multi answer poll with 2 answers", "Single answer poll with 1 answers"], embed.Fields.Select(field => field.Value));
    }

    [Fact]
    public void PresetList_CreateEmbed_GivesEachPresetItsStateKindAndDescription()
    {
        List<WeeklyPollOptionPresetResource> presets =
        [
            new() { Name = "Days Of Week", Description = "The coming week", IsActive = true, IsSpecialPreset = true },
            new() { Name = "Retired", Description = "Old", IsActive = false }
        ];

        Embed embed = Assert.Single(PollPresetListEmbedProcessor.CreateEmbed(presets));

        Assert.Equal("Weekly Poll Presets:", embed.Title);
        Assert.Equal(
            ["Active Special option preset\nDescription: The coming week", "Inactive Standard option preset\nDescription: Old"],
            embed.Fields.Select(field => field.Value));
    }
}
