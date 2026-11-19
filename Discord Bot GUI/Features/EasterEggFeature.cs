using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Core;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Features;

public class EasterEggFeature(
    IGreetingService greetingService,
    IDiscordClient client,
    Random random,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IGreetingService greetingService = greetingService;
    private readonly IDiscordClient client = client;
    private readonly Random random = random;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Response to mention: <@id>, or the older nickname form <@!id> that some messages still use
            ulong botId = client.CurrentUser.Id;
            if (Context.Message.Content.Contains($"<@{botId}>", StringComparison.Ordinal) || Context.Message.Content.Contains($"<@!{botId}>", StringComparison.Ordinal))
            {
                List<GreetingResource> list = await greetingService.GetAllGreetingAsync();
                if (!CollectionTools.IsNullOrEmpty(list))
                {
                    _ = await Context.Channel.SendMessageAsync(list[random.Next(0, list.Count)].Url);
                    return false;
                }
            }

            //Easter egg messages
            if (random.Next(0, 5000) == 0)
            {
                _ = await Context.Channel.SendMessageAsync(Constant.EasterEggMessages[random.Next(0, Constant.EasterEggMessages.Length)]);
            }
            else if (random.Next(0, 20) == 0)
            {
                //No guild in a direct message
                MessageReference refer = new(Context.Message.Id, Context.Channel.Id, Context.Guild?.Id, false);
                string mess = Context.Message.Content.ToLower();
                if (mess.StartsWith("i think", StringComparison.Ordinal))
                {
                    string reply = random.Next(0, 2) == 0 ? "I agree wholeheartedly!" : Context.Message.Content.ToMockText();
                    _ = await Context.Channel.SendMessageAsync(reply, messageReference: refer);
                }
                else if ((mess.StartsWith("i am", StringComparison.Ordinal) && mess != "i am") || (mess.StartsWith("i'm", StringComparison.Ordinal) && mess != "i'm"))
                {
                    string message = string.Concat("Hey ", Context.Message.Content.AsSpan(mess.StartsWith("i am", StringComparison.Ordinal) ? 5 : 4), ", I'm Kim Synthji!");
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
