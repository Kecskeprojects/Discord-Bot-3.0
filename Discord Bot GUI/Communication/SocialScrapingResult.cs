using System.Collections.Generic;

namespace Discord_Bot.Communication;

public class SocialScrapingResult
{
    public SocialScrapingResult()
    {
        ErrorMessage = "";
        TextContent = "";
        Content = [];
    }

    public SocialScrapingResult(string message)
    {
        ErrorMessage = message;
        TextContent = "";
        Content = [];
    }

    public List<MediaContent> Content { get; set; }
    public string ErrorMessage { get; set; }
    public string TextContent { get; set; }
}
