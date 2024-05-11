﻿using Discord;
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
                else if (r.Next(0, 20) == 0)
                {
                    MessageReference refer = new(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
                    string mess = Context.Message.Content.ToLower();
                    if (mess.StartsWith("i think"))
                    {
                        if (r.Next(0, 2) == 0)
                        {
                            await Context.Channel.SendMessageAsync("I agree wholeheartedly!", messageReference: refer);
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync(Context.Message.Content.ToMockText(), messageReference: refer);
                        }
                    }
                    else if ((mess.StartsWith("i am") && mess != "i am") || (mess.StartsWith("i'm") && mess != "i'm"))
                    {
                        string message = string.Concat("Hey ", Context.Message.Content.AsSpan(mess.StartsWith("i am") ? 5 : 4), ", I'm Kim Synthji!");
                        await Context.Channel.SendMessageAsync(message, messageReference: refer);
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