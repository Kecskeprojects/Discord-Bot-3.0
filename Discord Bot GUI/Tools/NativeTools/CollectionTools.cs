using Discord_Bot.Communication;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Discord_Bot.Tools.NativeTools;

public static class CollectionTools
{
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] T? collection) where T : ICollection
    {
        return collection == null || collection.Count == 0;
    }

    public static List<MusicRequest> ShufflePlaylist(List<MusicRequest> playlist)
    {
        //Copy the server's playlist without the currently playing song, saving that song for later
        MusicRequest nowPlaying = playlist[0];
        List<MusicRequest> current = playlist.Skip(1).ToList();

        List<MusicRequest> shuffled = [];
        int length = current.Count;
        Random r = new();

        //Go through the entire playlist once
        for (int i = 0; i < length; i++)
        {
            //generate a random number, accounting for the slowly depleting current playlist
            int index = r.Next(0, current.Count);

            //Adding the randomly chosen song and removing it from the original list
            shuffled.Add(current[index]);
            current.RemoveAt(index);
        }

        //Adding back the currently playing song to the beginning and switching it out with the unshuffled one
        shuffled.Insert(0, nowPlaying);
        return shuffled;
    }
}
