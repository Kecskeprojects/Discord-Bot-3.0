using Discord_Bot.Services.Models.Wotd;

namespace Discord_Bot.Tests.Common.Builders;

//What the bot's external services return, built for the tests that replace those services
public static class ServiceModels
{
    //A day of wotd.transparent.com's feed
    public static WotdBase WordOfTheDay(string language)
    {
        return new WotdBase
        {
            Words = new Words
            {
                Langname = language,
                Word = "사랑",
                Wordsound = "https://example.com/word.mp3",
                Translation = "love",
                Fnphrase = "사랑해요",
                Phrasesound = "https://example.com/phrase.mp3",
                Enphrase = "I love you"
            }
        };
    }
}
