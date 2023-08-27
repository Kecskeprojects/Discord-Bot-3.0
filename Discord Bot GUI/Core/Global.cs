﻿using Discord;
using Discord.Commands;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace Discord_Bot.Core
{
    public static class Global
    {
        #region Global variables
        public static bool InstagramChecker { get; set; }

        public static bool TwitterChecker { get; set; }

        //Extendable easter egg message list
        public static readonly string[] EasterEggMessages = new string[]
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
                "I hear voices while you sleep"
            };
        #endregion

        #region Global Functions
        //Check if user has a nickname
        public static string GetNickName(SocketCommandContext context)
        {
            return context.Channel.GetChannelType() != ChannelType.DM
                ? (context.User as Discord.WebSocket.SocketGuildUser).Nickname ?? context.User.Username
                : context.User.Username;
        }

        //Check if server has a type of channel, and if yes, is it on the list
        public static bool IsTypeOfChannel(ServerResource server, ChannelTypeEnum type, ulong channelId, bool allowLackOfType = false) =>
            !server.SettingsChannels.ContainsKey(type) ? allowLackOfType : server.SettingsChannels[type].Contains(channelId);

        public static Stream GetStream(string url)
        {
            Stream imageData = null;

            using (HttpClient wc = new())
            {
                imageData = wc.GetStreamAsync(url).GetAwaiter().GetResult();
            }

            return imageData;
        }

        //Testing connection by pinging google, it is quite a problem if that's down too
        public static bool Connection()
        {
            try
            {
                if (new Ping().Send("google.com", 1000, new byte[32], new PingOptions()).Status == IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }

        public static string CurrentTime(bool utc = false)
        {
            DateTime curr = utc ? DateTime.UtcNow : DateTime.Now;

            string hour = curr.Hour < 10 ? "0" + curr.Hour.ToString() : curr.Hour.ToString();
            string minute = curr.Minute < 10 ? "0" + curr.Minute.ToString() : curr.Minute.ToString();
            string second = curr.Second < 10 ? "0" + curr.Second.ToString() : curr.Second.ToString();

            return $"{hour}:{minute}:{second}";
        }

        public static string CurrentDate(bool utc = false)
        {
            DateTime curr = utc ? DateTime.UtcNow : DateTime.Now;

            string year = curr.Year.ToString();
            string month = curr.Month < 10 ? "0" + curr.Month.ToString() : curr.Month.ToString();
            string day = curr.Day < 10 ? "0" + curr.Day.ToString() : curr.Day.ToString();

            return $"{year}-{month}-{day}";
        }
        #endregion
    }
}
