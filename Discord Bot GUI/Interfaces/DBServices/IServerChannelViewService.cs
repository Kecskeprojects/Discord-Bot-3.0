using Discord_Bot.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IServerChannelViewService
    {
        Task<Dictionary<ChannelTypeEnum, List<ulong>>> GetServerChannelsAsync(int serverId);
    }
}
