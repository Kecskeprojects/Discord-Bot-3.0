using System.Collections.Generic;

namespace Discord_Bot.Enums
{
    public class StaticLists
    {
        public static string[] EasterEggMessages { get; } = new string[]
            {
                "I know where you live",
                "It is so dark in here",
                "Who are you?",
                "It is time",
                "Are you sure about this?",
                "Meow...?",
                "I love you all",
                "I so so want to get some takeout for dinner",
                ":rabbit:",
                "Happy birthday",
                "I could go for some macarons rn",
                "Yes baby yes",
                "I am sorry?",
                "It's so over for you",
                "Sus",
                "I will not let you off that easy",
                "I hear voices while you sleep",
                "I am what you are most afraid of",
                "I watch you sleep",
                "Do you want me to tell you a joke?"
            };

        //Array for 8ball answers
        public static string[] Answers8ball { get; } =
            {
                "It is certain",
                "It is decidedly so",
                "Without a doubt",
                "Yes, definitely",
                "You may rely on it",
                "As I see it, yes",
                "Most likely",
                "Outlook good",
                "Yes",
                "Signs point to yes",
                "Reply hazy try again",
                "Ask again later",
                "Better not tell you now",
                "Cannot predict now",
                "Concentrate and ask again",
                "Don't count on it",
                "My reply is no",
                "My sources say no",
                "Outlook not so good",
                "Very doubtful"
            };

        public static Dictionary<string, string> WotdLanguages { get; } = new()
        {
            {"mandarin", "zh-widget.xml"},
            {"dutch", "nl-widget.xml"},
            {"portuguese_brazilian", "pt-widget.xml"},
            {"latin", "la-widget.xml"},
            {"spanish", "es-widget.xml"},
            {"esperanto", "esp-widget.xml"},
            {"irish", "ga-widget.xml"},
            {"french", "fr-widget.xml"},
            {"balinese", "bal-eng-widget.xml"},
            {"russian", "ru-widget.xml"},
            {"dari", "dari-widget.xml"},
            {"swedish", "swedish-widget.xml"},
            {"hindi", "hindi-widget.xml"},
            {"polish", "pl-widget.xml"},
            {"turkish", "turkish-widget.xml"},
            {"korean", "korean-widget.xml"},
            {"urdu", "urdu-widget.xml"},
            {"german", "de-widget.xml"},
            {"hebrew", "hebrew-widget.xml"},
            {"indonesian", "indonesian-widget.xml"},
            {"norwegian", "norwegian-widget.xml"},
            {"arabic", "ar-widget.xml"},
            {"italian", "it-widget.xml"},
            {"pashto", "pashto-widget.xml"},
            {"japanese", "ja-widget.xml"}
        };

        //Array for birthday messages
        public static string[] BirthdayMessage { get; } =
            {
                "Hey look, {0} has their birthday today! Send some nice wishes!\n{1}",
                "Oh wow, it's birthday time! Happy birthday {0}!\n{1}",
                "It's {0} day today!\n{1}",
                "Happy birthday {0}!\n{1}",
                "The day of reckoning is upon us, it's {0}'s birthday today!\n{1}",
                "Get the cakes out bitches, it's {0} day!\n{1}",
                "Happy birthday {0}! I am suddenly hungry for some cakes...\n{1}",
                "Yay, what a special day, it's {0}'s birthday!\n{1}"
            };
    }
}
