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
    public void ReplaceExhaustedKey_GoesThroughEveryKeyOnce()
    {
        keys.Reset(["a", "b", "c"]);

        List<string> used = [];
        for (string? key = keys.CurrentKey; key != null; key = keys.ReplaceExhaustedKey(key))
        {
            used.Add(key);
        }

        string[] expected = ["a", "b", "c"];
        Assert.Equal(expected, used.Order());
    }

    [Fact]
    public void ReplaceExhaustedKey_ForAKeyThatIsNotCurrent_KeepsTheCurrentOne()
    {
        keys.Reset(["a", "b"]);
        string current = keys.CurrentKey!;
        string other = current == "a" ? "b" : "a";

        Assert.Equal(current, keys.ReplaceExhaustedKey(other));
        Assert.Null(keys.ReplaceExhaustedKey(current));
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
