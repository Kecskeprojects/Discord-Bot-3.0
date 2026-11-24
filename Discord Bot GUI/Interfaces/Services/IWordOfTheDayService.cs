using Discord_Bot.Services.Models.Wotd;

namespace Discord_Bot.Interfaces.Services;

public interface IWordOfTheDayService
{
    //Null for a language without a feed, and when the site fails or sends something other than the feed (logged)
    Task<WotdBase?> GetDataAsync(string language);
}
