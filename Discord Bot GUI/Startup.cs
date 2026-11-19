using DatabaseORM;
using DatabaseORM.Interface;
using DatabaseORM.Mapping;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Features;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.Processors;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors;
using Discord_Bot.Processors.EmbedProcessors.LastFm;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Services;
using Discord_Bot.Windows;
using LastFmApi;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace Discord_Bot;

public static class Startup
{
    public static IServiceProvider Run()
    {
        IServiceCollection collection = new ServiceCollection();
        ConfigureServices(collection);

        return collection.BuildServiceProvider();
    }

    /// <summary>
    /// Registers everything the bot uses, apart from building the provider (<see cref="Run"/>),
    /// so tests can reuse the real registrations and replace only the outside world.
    /// </summary>
    public static void ConfigureServices(IServiceCollection collection)
    {
        DiscordSocketClient client = new(new DiscordSocketConfig()
        {
            EnableVoiceDaveEncryption = true,
            //Only the intents the bot uses, each one adds gateway events that have to be received and cached
            //Not needed: reaction, typing, ban, emoji, webhook, integration and poll events, none are handled
            //(adding reactions and sending polls go through the REST API and do not need intents)
            GatewayIntents = GatewayIntents.Guilds
                            | GatewayIntents.GuildMembers       //Member cache for DownloadUsersAsync, nicknames, server member filtering (privileged)
                            | GatewayIntents.GuildVoiceStates   //Music playback
                            | GatewayIntents.GuildMessages
                            | GatewayIntents.DirectMessages
                            | GatewayIntents.MessageContent,    //Text commands (privileged)
            LogLevel = LogSeverity.Info
        });

        InteractionService interactions = new(client,
            new InteractionServiceConfig()
            {
                DefaultRunMode = Discord.Interactions.RunMode.Sync,
                UseCompiledLambda = true,
            });

        CommandService commands = new(new CommandServiceConfig()
        {
            DefaultRunMode = Discord.Commands.RunMode.Sync
        });

        //Core
        _ = collection.AddSingleton(client);
        _ = collection.AddSingleton<IDiscordClient>(client); //What features and tools use, so tests can replace it
        _ = collection.AddSingleton(interactions);
        _ = collection.AddSingleton(commands);
        _ = collection.AddTransient<IConfig, Config>(); //Configuration can be edited without stopping the program
        _ = collection.AddSingleton(TimeProvider.System); //Tests replace it with a FakeTimeProvider
        _ = collection.AddSingleton(Random.Shared); //Thread-safe; tests replace it to choose which random branch runs
        //In-memory state shared by every command, interaction and feature, lost on restart
        _ = collection.AddSingleton<AudioStateStore>();
        _ = collection.AddSingleton<BiasGameStore>();
        _ = collection.AddSingleton<YoutubeKeyStore>();
        //One logger: the bot's code gets IBotLogger, App and BotWindow the concrete type (LogToFile, LogAdded)
        BotLogger logger = new();
        _ = collection.AddSingleton(logger);
        _ = collection.AddSingleton<IBotLogger>(logger);

        //Database layer: DbContext, server cache, DB services and repositories
        DatabaseStartup.ConfigureDatabaseServices(collection, provider => provider.GetRequiredService<IConfig>().SqlConnectionString);

        TypeAdapterConfig mapsterConfig = new();
        new DatabaseMapperConfig().Register(mapsterConfig);
        new MapperConfig().Register(mapsterConfig);
        mapsterConfig.Compile(); //Fail fast on startup if a mapping is invalid
        _ = collection.AddSingleton(mapsterConfig);
        _ = collection.AddSingleton<IMapper, Mapper>();

        _ = collection.AddSingleton<BotMain>();
        _ = collection.AddSingleton<InteractionHandler>();
        _ = collection.AddSingleton<CommandHandler>();
        _ = collection.AddSingleton<BotWindow>();

        //Processors
        _ = collection.AddTransient<IWhoKnowsImageProcessor, WhoKnowsImageProcessor>();
        _ = collection.AddTransient<BonkGifProcessor>();
        _ = collection.AddTransient<BiasGameImageProcessor>();
        _ = collection.AddTransient<BiasGameWinnerBracketImageProcessor>();
        _ = collection.AddTransient<BiasScrapingProcessor>();
        _ = collection.AddTransient<LastFmWhoKnowsEmbedProcessor>();

        //Features
        _ = collection.AddTransient<InstagramEmbedFeature>();
        _ = collection.AddTransient<SelfRoleFeature>();
        _ = collection.AddTransient<EasterEggFeature>();
        _ = collection.AddTransient<CustomCommandFeature>();
        _ = collection.AddTransient<BirthdayFeature>();
        _ = collection.AddTransient<ReminderFeature>();
        _ = collection.AddTransient<TwitchNotificationFeature>();
        _ = collection.AddTransient<YoutubeAddPlaylistFeature>();
        _ = collection.AddTransient<AudioPlayFeature>();
        _ = collection.AddTransient<AudioRequestFeature>();
        _ = collection.AddTransient<UnmuteFeature>();
        _ = collection.AddTransient<WeeklyPollFeature>();

        //Services
        _ = collection.AddSingleton<IBrowserService, BrowserService>();
        _ = collection.AddSingleton<IWebService, WebService>();
        _ = collection.AddTransient<ITwitchAPI, TwitchAPI>();
        _ = collection.AddTransient<ITwitchCLI, TwitchCLI>();
        _ = collection.AddTransient<ISpotifyAPI, Services.SpotifyAPI>();
        _ = collection.AddSingleton<ISpotifyClientFactory, SpotifyClientFactory>();
        _ = collection.AddTransient<IYoutubeAPI, YoutubeAPI>();
        _ = collection.AddSingleton<IYoutubeDataClient, YoutubeDataClient>();
        //collection.AddTransient<IInstaLoader, InstaLoader>();
        _ = collection.AddTransient<IInstaScraper, InstaScraper>();
        _ = collection.AddTransient<IWordOfTheDayService, WordOfTheDayService>();
        _ = collection.AddTransient<IYoutubeDownloadService, YoutubeStreamService>();
        _ = collection.AddTransient<ILastFmAPI, LastFmAPI>();
        _ = collection.AddTransient<IMusicBrainzAPI, MusicBrainzAPI>();
        _ = collection.AddTransient<ITwitterScraper, TwitterScraper>();
        _ = collection.AddTransient<IKpopDbScraper, KpopDbScraper>();

        //Last.fm: one long-lived RestClient, as RestSharp recommends; tests replace BaseRequests to fake the HTTP layer
        _ = collection.AddSingleton(new BaseRequests(new RestClient(LastFmApi.Constant.LastFmApiBaseUri)));
        _ = collection.AddSingleton<UserBasedRequests>();
        _ = collection.AddSingleton<InfoBasedRequests>();
        _ = collection.AddSingleton<UtilityRequests>();
        _ = collection.AddSingleton<WhoKnowsRequests>();

        _ = collection.AddLogging();
    }
}
