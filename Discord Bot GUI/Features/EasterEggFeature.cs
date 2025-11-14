using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class EasterEggFeature(
    IGreetingService greetingService,
    DiscordSocketClient client,
    IServerService serverService,
    BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IGreetingService greetingService = greetingService;
    private readonly DiscordSocketClient client = client;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Response to mention
            if (Context.Message.Content.Contains(client.CurrentUser.Mention) || Context.Message.Content.Contains(client.CurrentUser.Mention.Remove(2, 1)))
            {
                List<GreetingResource> list = await greetingService.GetAllGreetingAsync();
                if (!CollectionTools.IsNullOrEmpty(list))
                {
                    _ = await Context.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].Url);
                    return false;
                }
            }

            //Easter egg messages
            Random r = new();
            if (r.Next(0, 5000) == 0)
            {
                _ = await Context.Channel.SendMessageAsync(Constant.EasterEggMessages[r.Next(0, Constant.EasterEggMessages.Length)]);
            }
            else if (r.Next(0, 20) == 0)
            {
                MessageReference refer = new(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
                string mess = Context.Message.Content.ToLower();
                if (mess.StartsWith("i think"))
                {
                    _ = r.Next(0, 2) == 0
                        ? await Context.Channel.SendMessageAsync("I agree wholeheartedly!", messageReference: refer)
                        : await Context.Channel.SendMessageAsync(Context.Message.Content.ToMockText(), messageReference: refer);
                }
                else if ((mess.StartsWith("i am") && mess != "i am") || (mess.StartsWith("i'm") && mess != "i'm"))
                {
                    string message = string.Concat("Hey ", Context.Message.Content.AsSpan(mess.StartsWith("i am") ? 5 : 4), ", I'm Kim Synthji!");
                    _ = await Context.Channel.SendMessageAsync(message, messageReference: refer);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("EasterEggFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }
}
