using Discord_Bot.Communication;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Discord_Bot.Tools.NativeTools;

public static class CollectionTools
{
    public static bool IsNullOrEmpty<T>(T collection) where T : ICollection
    {
        return collection == null || collection.Count == 0;
    }
    public static List<MusicRequest> ShufflePlaylist(List<MusicRequest> playlist)
    {
        //Get the server's playlist, and remove the currently playing song, but saving it for later
        List<MusicRequest> current = [.. playlist];
        MusicRequest nowPlaying = current[0];
        current.RemoveAt(0);

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
