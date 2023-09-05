using System;
using System.Collections.Generic;

namespace Discord_Bot.Communication
{
    public class TwitterScrapingResult
    {
        public TwitterScrapingResult(string message)
        {
            ErrorMessage = message;
            Videos = new();
            Images = new();
        }
        public TwitterScrapingResult(List<Uri> videos, List<Uri> images)
        {
            Videos = videos;
            Images = images;
        }
        public TwitterScrapingResult(List<Uri> videos, List<Uri> images, string message)
        {
            ErrorMessage = message;
            Videos = videos;
            Images = images;
        }

        public List<Uri> Videos { get; set; }
        public List<Uri> Images { get; set; }
        public string ErrorMessage { get; set; }
    }
}
