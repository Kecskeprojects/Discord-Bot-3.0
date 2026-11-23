using Discord_Bot.Communication;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Tests.Unit.Bot.Tools.NativeTools;

public sealed class CollectionToolsTests
{
    [Fact]
    public void IsNullOrEmpty_NullOrEmptyOrNot()
    {
        Assert.True(CollectionTools.IsNullOrEmpty((List<int>?) null));
        Assert.True(CollectionTools.IsNullOrEmpty(new List<int>()));
        Assert.False(CollectionTools.IsNullOrEmpty(new List<int> { 1 }));
    }

    //!shuffle: the order is random, so this checks what any order must keep, over several runs
    [Fact]
    public void ShufflePlaylist_KeepsTheSongPlayingFirstAndEverySongOnce()
    {
        List<MusicRequest> playlist = [.. Enumerable.Range(1, 10).Select(i => new MusicRequest($"https://example.com/{i}", $"Song {i}", "", "03:00", "tester"))];
        HashSet<MusicRequest> songs = [.. playlist];

        for (int run = 0; run < 20; run++)
        {
            List<MusicRequest> shuffled = CollectionTools.ShufflePlaylist(playlist);

            Assert.Same(playlist[0], shuffled[0]);
            Assert.Equal(playlist.Count, shuffled.Count);
            Assert.True(songs.SetEquals(shuffled));
        }
        Assert.Equal(Enumerable.Range(1, 10).Select(i => $"Song {i}"), playlist.Select(song => song.Title));
    }
}
