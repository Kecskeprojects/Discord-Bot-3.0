using System;
using System.Collections.Generic;

namespace Discord_Bot.Communication
{
    public class TwitterScrapingResult
    {
        public TwitterScrapingResult()
        {
            ErrorMessage = "";
            TextContent = "";
            Videos = [];
            Images = [];
        }

        public TwitterScrapingResult(string message)
        {
            ErrorMessage = message;
            TextContent = "";
            Videos = [];
            Images = [];
        }

        public TwitterScrapingResult(List<Uri> videos, List<Uri> images, string textContent)
        {
            TextContent = textContent;
            Videos = videos;
            Images = images;
        }

        public TwitterScrapingResult(List<Uri> videos, List<Uri> images, string textContent, string message)
        {
            ErrorMessage = message;
            TextContent = textContent;
            Videos = videos;
            Images = images;
        }

        public List<Uri> Videos { get; set; }
        public List<Uri> Images { get; set; }
        public string ErrorMessage { get; set; }
        public string TextContent { get; set; }
    }
}
