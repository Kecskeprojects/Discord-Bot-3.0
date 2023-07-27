﻿using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    internal interface IVoiceCommands
    {
        [Command("p")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "play" })]
        public Task Play([Remainder] string content);


        [Command("join")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "connect", "conn" })]
        public Task Join();


        [Command("leave")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "disconnect", "disconn", "disc", "dc" })]
        public Task Leave();


        [Command("queue")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "q" })]
        public Task Queue(int index);


        [Command("np")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "now playing", "nowplaying" })]
        public Task Now_Playing();


        [Command("clear")]
        [RequireContext(ContextType.Guild)]
        public Task Clear();


        [Command("skip")]
        [RequireContext(ContextType.Guild)]
        public Task Skip();


        [Command("remove")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "r" })]
        public Task Remove(int position);


        [Command("shuffle")]
        [RequireContext(ContextType.Guild)]
        public Task Shuffle();
    }
}
