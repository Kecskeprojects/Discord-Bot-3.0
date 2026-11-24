using Discord_Bot.Core;

namespace Discord_Bot.Tests.Unit.Bot.Core;

public sealed class YoutubeKeyStoreTests
{
    private readonly YoutubeKeyStore keys = new();

    [Fact]
    public void CurrentKey_BeforeTheFirstReset_IsNull()
    {
        Assert.Null(keys.CurrentKey);
    }

    [Fact]
    public void CurrentKey_StartsWithTheConfigurationsFirstKey()
    {
        keys.Reset(["b", "a"]);

        Assert.Equal("b", keys.CurrentKey);
    }

    //Switching on every request would spread the quota over every key; one key is used until it's spent
    [Fact]
    public void AddUsage_BelowTheDailyQuota_KeepsTheKey()
    {
        keys.Reset(["a", "b"]);

        keys.AddUsage("a", 9_999);

        Assert.Equal("a", keys.CurrentKey);
    }

    [Fact]
    public void AddUsage_ReachingTheDailyQuota_SwitchesToTheNextKey()
    {
        keys.Reset(["a", "b"]);
        keys.AddUsage("a", 9_900);

        keys.AddUsage("a", 100);

        Assert.Equal("b", keys.CurrentKey);
        //Dropped until the next refill, like a key YouTube refused
        Assert.Null(keys.ReplaceExhaustedKey("b"));
    }

    [Fact]
    public void ReplaceExhaustedKey_GoesThroughEveryKeyOnce()
    {
        keys.Reset(["a", "b", "c"]);

        List<string> used = [];
        for (string? key = keys.CurrentKey; key != null; key = keys.ReplaceExhaustedKey(key))
        {
            used.Add(key);
        }

        Assert.Equal(["a", "b", "c"], used);
    }

    [Fact]
    public void ReplaceExhaustedKey_ForAKeyThatIsNotCurrent_KeepsTheCurrentOne()
    {
        keys.Reset(["a", "b"]);

        Assert.Equal("a", keys.ReplaceExhaustedKey("b"));
        Assert.Null(keys.ReplaceExhaustedKey("a"));
    }

    [Fact]
    public void Reset_CountsADuplicateKeyOnce()
    {
        keys.Reset(["a", "a"]);

        Assert.Equal("a", keys.CurrentKey);
        Assert.Null(keys.ReplaceExhaustedKey("a"));
    }

    [Fact]
    public void AddUsage_ForADroppedKey_DoesNotBringItBack()
    {
        keys.Reset(["a"]);
        keys.ReplaceExhaustedKey("a");

        keys.AddUsage("a", 100);

        Assert.Null(keys.CurrentKey);
    }

    [Fact]
    public void Reset_RefillsTheKeysAfterTheQuotaRanOut()
    {
        keys.Reset(["a"]);
        keys.ReplaceExhaustedKey("a");

        keys.Reset(["a"]);

        Assert.Equal("a", keys.CurrentKey);
    }
}
