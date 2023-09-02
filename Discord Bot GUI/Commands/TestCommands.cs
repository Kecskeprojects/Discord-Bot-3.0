using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class TestCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IYoutubeAPI youtubeAPI;
        private readonly Logging logger;

        public TestCommands(IYoutubeAPI youtubeAPI, Logging logger)
        {
            this.youtubeAPI = youtubeAPI;
            this.logger = logger;
        }

        [Command("Test")]
        public async Task Test()
        {
            try
            {
                await youtubeAPI.Searching("https://www.youtube.com/watch?v=5dlVOyH4lZg&list=PLbdSi72ah3pv1gosJUS1coGzST7SvWby3&index=1", Global.GetNickName(Context.Channel, Context.User), Context.Guild.Id, Context.Channel.Id);
            }
            catch (Exception ex)
            {
                logger.Error("TestCommands.cs Test", ex.ToString());
            }
        }

    }
}
