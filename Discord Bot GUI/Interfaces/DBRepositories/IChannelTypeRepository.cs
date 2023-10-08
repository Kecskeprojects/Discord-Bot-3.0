using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IChannelTypeRepository
    {
        Task<ChannelType> GetChannelTypeByIdAsync(ChannelTypeEnum channelTypeId);
    }
}
