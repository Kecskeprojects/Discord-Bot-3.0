﻿using System.Collections.Generic;

namespace Discord_Bot.Resources;
public class WeeklyPollResource
{
    public ulong ServerDiscordId { get; set; }
    public ulong ChannelDiscordId { get; set; }
    public ulong RoleDiscordId { get; set; }
    public bool IsMultipleAnswer { get; set; }
    public string Title { get; set; }
    public long CloseInTimeSpanTicks { get; set; }

    public List<string> Options { get; set; }
}
