using AutoMapper;
using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Features;
public class WeeklyPollFeature(
    IWeeklyPollService weeklyPollService,
    DiscordSocketClient client,
    IServerService serverService,
    IMapper mapper,
    BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IWeeklyPollService weeklyPollService = weeklyPollService;
    private readonly DiscordSocketClient client = client;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of polls that are sent out on this day
            DayOfWeek dayOfWeek = DateTime.UtcNow.DayOfWeek;
            List<WeeklyPollResource> result = await weeklyPollService.GetPollsByDayOfWeekAsync(dayOfWeek);

            if (!CollectionTools.IsNullOrEmpty(result))
            {
                foreach (WeeklyPollResource poll in result)
                {
                    PollProperties pollProp = mapper.Map<PollProperties>(poll);

                    SocketGuild server = client.GetGuild(poll.ServerDiscordId);

                    if (server?.GetChannel(poll.ChannelDiscordId) is ISocketMessageChannel channel)
                    {
                        SocketRole role = server.GetRole(poll.RoleDiscordId);
                        string notifRole = role != null ? $"<@&{role.Id}>" : "";

                        await channel.SendMessageAsync(notifRole, poll: pollProp);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("ReminderFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }
}
