namespace Discord_Bot.Tests.Common.Constants;

public static class TestTimes
{
    //The "now" of every test with a clock: a Wednesday afternoon in UTC, with seconds, so a test can see them dropped
    public static readonly DateTimeOffset Now = new(2026, 11, 18, 14, 5, 30, TimeSpan.Zero);
}
