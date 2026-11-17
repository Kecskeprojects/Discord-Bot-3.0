using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IIdolImageService
{
    Task<DbProcessResultEnum> AddOverrideAsync(int idolId, string imageUrl);
    Task<DbProcessResultEnum> RemoveIdolImagesAsync(string idol, string group);
}
