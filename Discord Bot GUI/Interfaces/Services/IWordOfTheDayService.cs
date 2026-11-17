using Discord_Bot.Services.Models.Wotd;

namespace Discord_Bot.Interfaces.Services;

public interface IWordOfTheDayService
{
    Task<WotdBase?> GetDataAsync(string language);
}
