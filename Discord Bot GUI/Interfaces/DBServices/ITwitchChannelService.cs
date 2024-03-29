﻿using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface ITwitchChannelService
    {
        Task<DbProcessResultEnum> AddNotificationRoleAsync(ulong serverId, ulong roleId, string roleName);
        Task<DbProcessResultEnum> AddTwitchChannelAsync(ulong serverId, string twitchUserId, string twitchUserName);
        Task<List<TwitchChannelResource>> GetChannelsAsync();
        Task<DbProcessResultEnum> RemoveNotificationRoleAsync(ulong serverId);
        Task<DbProcessResultEnum> RemoveTwitchChannelAsync(ulong serverId, string name);
        Task<DbProcessResultEnum> RemoveTwitchChannelsAsync(ulong serverId);
    }
}
