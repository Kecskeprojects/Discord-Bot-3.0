using Discord_Bot.Communication;

namespace Discord_Bot.Interfaces.Services;

public interface ITwitterScraper
{
    Task<SocialScrapingResult> GetDataFromUrls(List<Uri> uris);
}
