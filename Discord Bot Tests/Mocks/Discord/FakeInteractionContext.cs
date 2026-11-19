using Discord;

namespace Discord_Bot.Tests.Mocks.Discord;

/// <summary>
/// An <see cref="IInteractionContext"/> for TestHost.RunInteractionAsync. It is also an <see cref="IRouteMatchContainer"/>:
/// without that, InteractionService can't read the * parts of custom IDs such as BiasGame_Setup_Gender_*_*.
/// </summary>
public sealed class FakeInteractionContext(IDiscordInteraction interaction, IUser user, IMessageChannel channel, IGuild? guild = null)
    : IInteractionContext, IRouteMatchContainer
{
    public IDiscordClient Client { get; } = Substitute.For<IDiscordClient>();
    //Null in a direct message, as in Discord.Net's own contexts
    public IGuild Guild { get; } = guild!;
    public IMessageChannel Channel { get; } = channel;
    public IUser User { get; } = user;
    public IDiscordInteraction Interaction { get; } = interaction;
    public IEnumerable<IRouteSegmentMatch> SegmentMatches { get; private set; } = [];

    public void SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches)
    {
        SegmentMatches = segmentMatches;
    }
}
