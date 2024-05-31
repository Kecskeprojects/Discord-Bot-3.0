using Discord.Commands;
using Discord_Bot.Core;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

//Todo: after reorganizing, check if anything in especially the longer commands can be moved into tools, processors, etc...
//Furthermore, at the moment this solution does not work for the TwitchNotif and YTAddPlaylist features, consider a truly universal approarch, or more functions
public abstract class BaseFeature(Logging logger)
{
    protected readonly Logging logger = logger;
    protected SocketCommandContext Context { get; private set; }

    public async Task Run()
    {
        await ExecuteCoreLogicAsync();
    }

    public async Task Run(SocketCommandContext context)
    {
        Context = context;

        await ExecuteCoreLogicAsync();
    }

    protected abstract Task ExecuteCoreLogicAsync();
}
