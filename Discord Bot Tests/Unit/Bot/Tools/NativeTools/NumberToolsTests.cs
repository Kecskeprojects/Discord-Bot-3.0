using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Tests.Unit.Bot.Tools.NativeTools;

public sealed class NumberToolsTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(0, true)]
    [InlineData(5, false)]
    public void IsNullOrZero_Int(int? value, bool expected)
    {
        Assert.Equal(expected, NumberTools.IsNullOrZero(value));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(0ul, true)]
    [InlineData(5ul, false)]
    public void IsNullOrZero_Ulong(ulong? value, bool expected)
    {
        Assert.Equal(expected, NumberTools.IsNullOrZero(value));
    }
}
