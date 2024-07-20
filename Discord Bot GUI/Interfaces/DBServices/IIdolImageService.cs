using Discord_Bot.Communication.Modal;
using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface IIdolImageService
{
    Task<DbProcessResultEnum> AddOverrideAsync(int idolId, OverrideImageModal modal);
    Task<DbProcessResultEnum> RemoveIdolImagesAsync(string idol, string group);
}
