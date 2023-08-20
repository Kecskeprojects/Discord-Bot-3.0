﻿using Discord;
using Discord.Commands;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace Discord_Bot.Core
{
    public static class Global
    {
        #region Global variables
        //Might not be needed
        //Server information stored in a dictionary, the key is the Context.Guild.Id, the value is a complex class
        public static readonly Dictionary<ulong, ServerResource> servers = new();

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
        //Returns true if command's channel is a music channel
        public static bool IsMusicChannel(SocketCommandContext context)
        {
            if (servers[context.Guild.Id].MusicChannels.Length == 0) return true;
            else return servers[context.Guild.Id].MusicChannels.Contains(context.Channel.Id);
        }


        //Check if user has a nickname, and if message was sent in a server or not
        public static string GetNickName(SocketCommandContext context)
        {
            //Only check for nickname if user is not using DMs
            if (context.Channel.GetChannelType() != ChannelType.DM)
            {
                //If user has a nickname, use that in the embed
                return (context.User as Discord.WebSocket.SocketGuildUser).Nickname ?? context.User.Username;
            }
            else return context.User.Username;
        }


        public static Stream GetStream(string url)
        {
            Stream imageData = null;

            using (var wc = new System.Net.Http.HttpClient())
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
        #endregion
    }
}