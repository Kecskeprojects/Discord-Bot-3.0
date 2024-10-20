using Discord_Bot.Communication;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services;
public interface IInstaScraper
{
    public Task<SocialScrapingResult> GetDataFromUrl(Uri uri);
}
