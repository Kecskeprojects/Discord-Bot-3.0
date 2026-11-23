using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Tests.Unit.Bot.Tools.NativeTools;

public sealed class StringToolsTests
{
    //Last.fm rankings and play counts ("your 21st play")
    [Theory]
    [InlineData("1", "1st")]
    [InlineData("2", "2nd")]
    [InlineData("3", "3rd")]
    [InlineData("4", "4th")]
    [InlineData("0", "0th")]
    [InlineData("11", "11th")]
    [InlineData("12", "12th")]
    [InlineData("13", "13th")]
    [InlineData("21", "21st")]
    [InlineData("22", "22nd")]
    [InlineData("101", "101st")]
    [InlineData("111", "111th")]
    [InlineData("112", "112th")]
    [InlineData("", "")]
    public void AddNumberPositionIdentifier_GivesTheEnglishOrdinal(string position, string expected)
    {
        Assert.Equal(expected, StringTools.AddNumberPositionIdentifier(position));
    }

    //What !mute and !remind read their duration with; DateTimeTools then pairs each amount with its unit
    [Theory]
    [InlineData("1d 2h 30m", "1|d|2|h|30|m")]
    [InlineData("10minutes", "10|minutes")]
    [InlineData("2 weeks 3 days", "2|weeks|3|days")]
    public void GetTimeMeasurements_SplitsAmountsAndUnits(string input, string expected)
    {
        Assert.Equal(expected.Split('|'), StringTools.GetTimeMeasurements(input));
    }
}
