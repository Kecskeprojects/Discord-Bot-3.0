using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Tests.Common.Constants;
using Microsoft.Extensions.Time.Testing;

namespace Discord_Bot.Tests.Unit.Bot.Core;

public sealed class BiasGameStoreTests
{
    private readonly FakeTimeProvider clock = new(TestTimes.Now);
    private readonly BiasGameStore store;

    public BiasGameStoreTests()
    {
        store = new BiasGameStore(clock);
    }

    [Fact]
    public void TryStart_WithoutAGame_StartsOneNow()
    {
        BiasGameData? game = store.TryStart(DiscordIds.User);

        Assert.NotNull(game);
        Assert.Equal(TestTimes.Now.UtcDateTime, game.StartedAt);
        Assert.True(store.TryGet(DiscordIds.User, out BiasGameData? stored));
        Assert.Same(game, stored);
    }

    [Fact]
    public void TryStart_WithinThirtyMinutes_ReturnsNull()
    {
        store.TryStart(DiscordIds.User);
        clock.Advance(TimeSpan.FromMinutes(29));

        Assert.Null(store.TryStart(DiscordIds.User));
    }

    [Fact]
    public void TryStart_AfterThirtyMinutes_ReplacesTheAbandonedGameAndDisposesIt()
    {
        BiasGameData first = store.TryStart(DiscordIds.User)!;
        clock.Advance(TimeSpan.FromMinutes(31));

        BiasGameData? second = store.TryStart(DiscordIds.User);

        Assert.NotNull(second);
        Assert.NotSame(first, second);
        Assert.Throws<ObjectDisposedException>(() => first.WinnerBracket.Length);
    }

    [Fact]
    public void TryStart_ForAnotherUser_IsNotBlocked()
    {
        store.TryStart(DiscordIds.User);

        Assert.NotNull(store.TryStart(DiscordIds.OtherUser));
    }

    [Fact]
    public void Take_HandsTheGameToTheCallerAndForgetsIt()
    {
        BiasGameData game = store.TryStart(DiscordIds.User)!;

        using (BiasGameData? taken = store.Take(DiscordIds.User))
        {
            Assert.Same(game, taken);
        }
        Assert.False(store.TryGet(DiscordIds.User, out _));
    }

    [Fact]
    public void Remove_ForgetsAndDisposesTheGame()
    {
        BiasGameData game = store.TryStart(DiscordIds.User)!;

        store.Remove(DiscordIds.User);

        Assert.False(store.TryGet(DiscordIds.User, out _));
        Assert.Throws<ObjectDisposedException>(() => game.WinnerBracket.Length);
    }
}
