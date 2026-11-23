using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tools.NativeTools;
using System.Globalization;

namespace Discord_Bot.Tests.Unit.Bot.Tools.NativeTools;

public sealed class DateTimeToolsTests
{
    private static readonly DateTime now = TestTimes.Now.UtcDateTime;

    [Fact]
    public void TryAddValuesToDate_DropsTheSecondsAndStaysUtc()
    {
        bool added = DateTimeTools.TryAddValuesToDate(now, ["5", "m"], out DateTime result);

        Assert.True(added);
        Assert.Equal(new DateTime(2026, 11, 18, 14, 10, 0, DateTimeKind.Utc), result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    //"M" is months and "m" is minutes
    [Theory]
    [InlineData("y", "2027-11-18 14:05")]
    [InlineData("M", "2026-12-18 14:05")]
    [InlineData("w", "2026-11-25 14:05")]
    [InlineData("d", "2026-11-19 14:05")]
    [InlineData("h", "2026-11-18 15:05")]
    [InlineData("m", "2026-11-18 14:06")]
    public void TryAddValuesToDate_AddsOneOfEachUnit(string unit, string expected)
    {
        bool added = DateTimeTools.TryAddValuesToDate(now, ["1", unit], out DateTime result);

        Assert.True(added);
        Assert.Equal(DateTime.ParseExact(expected, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), result);
    }

    [Fact]
    public void TryAddValuesToDate_AddsSeveralAmountsInTheirOrder()
    {
        bool added = DateTimeTools.TryAddValuesToDate(now, ["1", "year", "2", "months", "3", "days", "30", "minutes"], out DateTime result);

        Assert.True(added);
        Assert.Equal(new DateTime(2028, 1, 21, 14, 35, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void TryAddValuesToDate_UnknownUnit_ReturnsFalse()
    {
        Assert.False(DateTimeTools.TryAddValuesToDate(now, ["2", "fortnights"], out _));
    }

    //Until 2026-11-23 the first four threw: a unit before its number ("h1" splits as "h", ""), a number too big for an int,
    //and amounts past the year 9999. The days of a week count were an int too, which could wrap around: 613566757 weeks gave 3 days
    [Theory]
    [InlineData("h", "")]
    [InlineData("99999999999", "h")]
    [InlineData("10000", "y")]
    [InlineData("400000000", "w")]
    [InlineData("613566757", "w")]
    public void TryAddValuesToDate_AmountItCantAdd_ReturnsFalse(string amount, string unit)
    {
        Assert.False(DateTimeTools.TryAddValuesToDate(now, [amount, unit], out _));
    }
}
