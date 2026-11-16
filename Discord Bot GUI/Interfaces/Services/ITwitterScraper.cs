using Discord_Bot.Communication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services;

public interface ITwitterScraper
{
    Task<SocialScrapingResult> GetDataFromUrls(List<Uri> uris);
}
