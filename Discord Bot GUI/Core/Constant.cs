using Discord_Bot.Communication.Bias;
using Discord_Bot.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discord_Bot.Core;

public static class Constant
{
    #region Urls
    public static readonly Uri DbKpopScrapeBaseUri = new("https://dbkpop.com/db/all-k-pop-idols/");
    public static readonly Uri MusicBrainzBaseUri = new("https://musicbrainz.org/ws/2/");
    public static readonly Uri WordOfTheDayBaseUri = new("http://wotd.transparent.com/rss/");

    public const string TwitchBaseUrl = "https://www.twitch.tv";
    public const string InstagramBaseUrl = "https://instagram.com";
    public const string LastFmIconUrl = "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png";
    public const string DbKpopBaseUrl = "https://dbkpop.com";
    public const string KProfilesBaseUrl = "https://kprofiles.com";
    public const string YoutubeBaseUrl = "https://www.youtube.com";
    public static readonly string[] TwitterBaseURLs =
    [
        "https://twitter.com/",
        "https://x.com/"
    ];
    #endregion

    #region Messages
    public static readonly string[] EasterEggMessages =
    [
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
    ];

    public static readonly string[] BiasGameStopMessages =
    [
        "You can't escape that easily",
        "No, you have to choose",
        "You know what you have to do",
        "Are you sure about that?"
    ];

    public static readonly string[] Answers8ball =
    [
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
    ];

    public static readonly string[] BirthdayMessage =
    [
        "Hey look, {0} has their birthday today! Send some nice wishes!",
        "Oh wow, it's birthday time! Happy birthday {0}!",
        "Happy birthday {0}!",
        "The day of reckoning is upon us, it's {0}'s birthday today and they are {1}!",
        "Get the cakes out, {0} is turning {1}!",
        "Happy birthday {0}! I am suddenly hungry for some cakes...",
        "Yay, what a special day, it's {0}'s birthday!",
        "Damn you are turning {1} huh, better get a cane now {0}.",
        "You are turning {1} {0}, you have the Big Bang on VHS don't you?"
    ];

    public static readonly string[] BiasExtraMessage =
    [
        "Between you and me, I quite like {0} too.",
        "{0}? Good choice!",
        "Hmm, this list is quite short, someone with a life over here...",
        "Oh, {0} is honestly just great."
    ];
    #endregion

    #region Utilities
    public static readonly char[] DateSeparator = [',', '/', '\\', '-', '.', ' '];
    public static readonly char[] WhiteSpaceSeparator = [' ', '\n'];
    public static readonly string[] SpecialCommandParameterDividers = ["-", ">", " or ", "  "];

    public static readonly string[] TwitterSmallSizingStrings =
    [
        "thumb",
        "small",
        "medium"
    ];

    public static readonly Dictionary<string, string> WotdLanguages = new()
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

    public static readonly StaticRoundData[] BiasGameStaticRoundData =
    [
        new StaticRoundData(baseLeftX: 5, baseRightX: 945, baseDiagonal: 50, baseY: 11, spacing: 54, totalPositions: 16),
        new StaticRoundData(baseLeftX: 100, baseRightX: 834, baseDiagonal: 66, baseY: 55, spacing: 141, totalPositions: 8),
        new StaticRoundData(baseLeftX: 196, baseRightX: 722, baseDiagonal: 82, baseY: 151, spacing: 332, totalPositions: 4),
        new StaticRoundData(baseLeftX: 292, baseRightX: 610, baseDiagonal: 98, baseY: 350, spacing: 0, totalPositions: 2),
        new StaticRoundData(baseLeftX: 435, baseRightX: 0, baseDiagonal: 130, baseY: 334, spacing: 0, totalPositions: 1),
    ];

    public static readonly ThreadState[] ActiveThreadStates =
    [
        /*ThreadState.Initialized,*/
        ThreadState.Ready,
        ThreadState.Running,
        ThreadState.Standby,
        /*ThreadState.Terminated,*/
        ThreadState.Wait,
        ThreadState.Transition,
        /*ThreadState.Unknown*/
    ];

    public static readonly ChannelTypeEnum[] RestrictedChannelTypes =
    [
        ChannelTypeEnum.RoleText,
        ChannelTypeEnum.TwitchNotificationText,
        ChannelTypeEnum.BirthdayText
    ];
    #endregion
}
