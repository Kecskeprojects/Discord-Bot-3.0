using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tools.NativeTools;
using MapsterMapper;

namespace Discord_Bot.Features;

public class WeeklyPollFeature(
    IWeeklyPollService weeklyPollService,
    IDiscordClient client,
    TimeProvider timeProvider,
    IServerService serverService,
    IMapper mapper,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IWeeklyPollService weeklyPollService = weeklyPollService;
    private readonly IDiscordClient client = client;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of polls that are sent out on this day
            DayOfWeek dayOfWeek = timeProvider.GetUtcNow().DayOfWeek;
            List<WeeklyPollResource> result = await weeklyPollService.GetPollsByDayOfWeekAsync(dayOfWeek);

            if (!CollectionTools.IsNullOrEmpty(result))
            {
                foreach (WeeklyPollResource poll in result)
                {
                    bool _ = await ProcessWeeklyPollAsync(poll);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }

    public async Task<bool> ProcessWeeklyPollAsync(WeeklyPollResource poll)
    {
        if (poll.Options.Count == 0)
        {
            return false;
        }

        PollProperties pollProp = mapper.Map<PollProperties>(poll);

        IGuild? server = await client.GetGuildAsync(poll.ServerDiscordId, CacheMode.CacheOnly);

        if (server != null && poll.ChannelDiscordId.HasValue
            && await server.GetChannelAsync(poll.ChannelDiscordId.Value, CacheMode.CacheOnly) is IMessageChannel channel)
        {
            IRole? role = poll.RoleDiscordId.HasValue ? server.GetRole(poll.RoleDiscordId.Value) : null;
            string notifRole = role != null ? $"<@&{role.Id}>" : "";

            IUserMessage message = await channel.SendMessageAsync(notifRole, poll: pollProp);
            if (poll.IsPinned)
            {
                await message.PinAsync();
            }
        }

        return true;
    }
}
