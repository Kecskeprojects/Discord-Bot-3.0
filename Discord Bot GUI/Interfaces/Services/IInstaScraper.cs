using Discord_Bot.Communication;

namespace Discord_Bot.Interfaces.Services;

public interface IInstaScraper
{
    Task<SocialScrapingResult> GetDataFromUrl(Uri uri);
}
