using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Core;
using Discord_Bot.Tests.Common.Tools;
using MapsterMapper;

namespace Discord_Bot.Tests.Unit.Bot.Core;

public sealed class MapperConfigTests
{
    private readonly IMapper mapper = TestMapper.Create(new MapperConfig());

    [Fact]
    public void Map_WeeklyPollToPoll_TakesTheQuestionAnswersAndDuration()
    {
        WeeklyPollResource poll = new()
        {
            Title = "Weekly question",
            Options = ["First", "Second"],
            IsMultipleAnswer = true,
            CloseInTimeSpanTicks = TimeSpan.FromDays(1).Ticks
        };

        PollProperties properties = mapper.Map<PollProperties>(poll);

        Assert.Equal("Weekly question", properties.Question.Text);
        string[] expected = ["First", "Second"];
        Assert.Equal(expected, properties.Answers.Select(answer => answer.Text));
        Assert.True(properties.AllowMultiselect);
        Assert.Equal(24u, properties.Duration);
        Assert.Equal(PollLayout.Default, properties.LayoutType);
    }
}
