using System.Collections.Generic;

namespace Discord_Bot.Communication;

public class TwitterScrapingResult
{
    public TwitterScrapingResult()
    {
        ErrorMessage = "";
        TextContent = "";
        Content = [];
    }

    public TwitterScrapingResult(string message)
    {
        ErrorMessage = message;
        TextContent = "";
        Content = [];
    }

    public List<TwitterContent> Content { get; set; }
    public string ErrorMessage { get; set; }
    public string TextContent { get; set; }
}
