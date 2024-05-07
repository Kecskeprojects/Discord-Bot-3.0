using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Features
{
    public class EasterEggFeature(IKeywordService keywordService, IGreetingService greetingService, DiscordSocketClient client, Logging logger) : BaseFeature(logger)
    {
        private readonly IKeywordService keywordService = keywordService;
        private readonly IGreetingService greetingService = greetingService;
        private readonly DiscordSocketClient client = client;

        protected override async Task ExecuteCoreLogicAsync()
        {
            try
            {
                //Response to mention
                if (Context.Message.Content.Contains(client.CurrentUser.Mention) || Context.Message.Content.Contains(client.CurrentUser.Mention.Remove(2, 1)))
                {
                    List<GreetingResource> list = await greetingService.GetAllGreetingAsync();
                    if (!CollectionTools.IsNullOrEmpty(list))
                    {
                        await Context.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].Url);
                        return;
                    }
                }

                //Response to keyword
                if (Context.Message.Content.Length <= 100 && Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    KeywordResource keyword = await keywordService.GetKeywordAsync(Context.Guild.Id, Context.Message.Content);
                    if (keyword != null)
                    {
                        await Context.Channel.SendMessageAsync(keyword.Response);
                        return;
                    }
                }

                //Easter egg messages
                Random r = new();
                if (r.Next(0, 5000) == 0)
                {
                    await Context.Channel.SendMessageAsync(StaticLists.EasterEggMessages[r.Next(0, StaticLists.EasterEggMessages.Length)]);
                }
                else if (r.Next(1, 21) < 1)
                {
                    string mess = Context.Message.Content.ToLower();
                    if (mess.StartsWith("i think"))
                    {
                        await Context.Channel.SendMessageAsync("I agree wholeheartedly!"); //Todo: Add a possibility to mock the user in a return message using randomized upper-lower characters, saying what they said
                    }
                    else if ((mess.StartsWith("i am") && mess != "i am") || (mess.StartsWith("i'm") && mess != "i'm"))
                    {
                        await Context.Channel.SendMessageAsync(string.Concat("Hey ", Context.Message.Content.AsSpan(mess.StartsWith("i am") ? 5 : 4), ", I'm Kim Synthji!"));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("EasterEggFeature.cs ExecuteCoreLogicAsync", ex);
            }
        }
    }
}
