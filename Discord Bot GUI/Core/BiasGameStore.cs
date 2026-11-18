using Discord_Bot.Communication.Bias;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Discord_Bot.Core;

/// <summary>
/// The running bias games, one per user. A game counts as abandoned 30 minutes after it started,
/// then its owner can start a new one. The store creates the games, so it disposes the ones it removes.
/// </summary>
public sealed class BiasGameStore(TimeProvider timeProvider)
{
    private readonly ConcurrentDictionary<ulong, BiasGameData> games = [];
    private readonly TimeProvider timeProvider = timeProvider;

    public bool TryGet(ulong userId, [NotNullWhen(true)] out BiasGameData? game)
    {
        return games.TryGetValue(userId, out game);
    }

    //Null when the user already has a game that is not abandoned yet
    public BiasGameData? TryStart(ulong userId)
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        if (games.TryGetValue(userId, out BiasGameData? running))
        {
            if (running.StartedAt > now.AddMinutes(-30))
            {
                return null;
            }

            //Only the abandoned game, in case another command of the same user replaced it meanwhile
            if (games.TryRemove(KeyValuePair.Create(userId, running)))
            {
                running.Dispose();
            }
        }

        BiasGameData game = new(userId, now);
        if (!games.TryAdd(userId, game))
        {
            //Another command of the same user started a game at the same moment
            game.Dispose();
            return null;
        }
        return game;
    }

    //Removes the user's game and hands it to the caller, who disposes it; null when there is none
    public BiasGameData? Take(ulong userId)
    {
        return games.TryRemove(userId, out BiasGameData? game) ? game : null;
    }

    //Ends a game that is finished or failed, and disposes it
    public void Remove(ulong userId)
    {
        Take(userId)?.Dispose();
    }
}
