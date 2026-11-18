using PuppeteerSharp;

namespace Discord_Bot.Interfaces.Services;

/// <summary>
/// The headless Chrome shared by the scrapers. It opens on the first page asked for.
/// </summary>
public interface IBrowserService
{
    Task CloseBrowser();
    Task<IPage> NewPage(params KeyValuePair<string, string>[] additionalHeaders);
}
