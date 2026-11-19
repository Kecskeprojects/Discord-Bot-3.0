using LastFmApi;
using LastFmApi.Enum;

namespace Discord_Bot.Tests.Unit.LastFmApi;

public sealed class LastFmHelperTests
{
    [Theory]
    [InlineData("all", "overall")]
    [InlineData("Week", "7day")]
    [InlineData("month", "1month")]
    [InlineData("quarter", "3month")]
    [InlineData("half", "6month")]
    [InlineData("12months", "12month")]
    public void LastfmTimePeriod_KnownWord_GivesLastFmsPeriod(string input, string expected)
    {
        bool known = LastFmHelper.LastfmTimePeriod(input, out string period);

        Assert.True(known);
        Assert.Equal(expected, period);
    }

    [Fact]
    public void LastfmTimePeriod_UnknownWord_ReturnsFalse()
    {
        bool known = LastFmHelper.LastfmTimePeriod("decade", out string period);

        Assert.False(known);
        Assert.Equal("", period);
    }

    [Theory]
    [InlineData(true, null, LastFmRequestResultEnum.Success)]
    [InlineData(false, "User not found", LastFmRequestResultEnum.Failure)]
    [InlineData(false, null, LastFmRequestResultEnum.EmptyResponse)]
    public void GetResultCode_FromTheResponseAndLastFmsMessage(bool hasResponse, string? message, LastFmRequestResultEnum expected)
    {
        Assert.Equal(expected, LastFmHelper.GetResultCode(hasResponse ? new object() : null, message));
    }
}
