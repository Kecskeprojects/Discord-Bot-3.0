using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IIdolImageService
    {
        Task<DbProcessResultEnum> RemoveIdolImagesAsync(string idol, string group);
    }
}
