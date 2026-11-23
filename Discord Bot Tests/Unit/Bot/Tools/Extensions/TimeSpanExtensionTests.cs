using Discord_Bot.Tools.Extensions;

namespace Discord_Bot.Tests.Unit.Bot.Tools.Extensions;

//A song's length in the music embeds
public sealed class TimeSpanExtensionTests
{
    [Theory]
    [InlineData(0, 3, 5, "03m:05s")]
    [InlineData(1, 2, 3, "1h:02m:03s")]
    [InlineData(0, 0, 0, "00m:00s")]
    public void ToTimeString_ShowsHoursOnlyWhenThereAreAny(int hours, int minutes, int seconds, string expected)
    {
        Assert.Equal(expected, new TimeSpan(hours, minutes, seconds).ToTimeString());
    }
}
