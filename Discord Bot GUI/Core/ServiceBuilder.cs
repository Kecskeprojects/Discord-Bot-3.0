using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Database;
using Discord_Bot.Database.DBRepositories;
using Discord_Bot.Database.DBServices;
using Discord_Bot.Features;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Services;
using Discord_Bot.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Discord_Bot.Core;

public static class ServiceBuilder
{
    public static IServiceProvider BuildService()
    {
        DiscordSocketClient client = new(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis
                            | GatewayIntents.GuildIntegrations | GatewayIntents.GuildWebhooks /*| GatewayIntents.GuildInvites*/ | GatewayIntents.GuildVoiceStates
                            /*| GatewayIntents.GuildPresences*/ | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping
                            | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessageTyping | GatewayIntents.MessageContent
                            /*| GatewayIntents.GuildScheduledEvents/* /*| GatewayIntents.AutoModerationConfiguration*/ /*| GatewayIntents.AutoModerationActionExecution*/
                            | GatewayIntents.GuildMessagePolls | GatewayIntents.DirectMessagePolls,
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

        IServiceCollection collection = new ServiceCollection();

        //Core
        collection.AddSingleton(client);
        collection.AddSingleton(interactions);
        collection.AddSingleton(commands);
        collection.AddTransient<Config>(); //This is transient meaning configuration can be edited on the fly
        collection.AddSingleton(new BotLogger());
        collection.AddSingleton(new Cache());

        Config config = new();
        collection.AddDbContext<MainDbContext>(options => options.UseSqlServer(config.SqlConnectionString));

        collection.AddAutoMapper(x => x.AddProfile<MapperConfig>());

        collection.AddSingleton<BotMain>();
        collection.AddSingleton<InteractionHandler>();
        collection.AddSingleton<CommandHandler>();
        collection.AddTransient<MainWindow>();

        //Processors
        collection.AddTransient<WhoKnowsImageProcessor>();
        collection.AddTransient<BonkGifProcessor>();
        collection.AddTransient<BiasGameImageProcessor>();
        collection.AddTransient<BiasGameWinnerBracketImageProcessor>();
        collection.AddTransient<BiasScrapingProcessor>();

        //Features
        collection.AddTransient<InstagramEmbedFeature>();
        collection.AddTransient<SelfRoleFeature>();
        collection.AddTransient<EasterEggFeature>();
        collection.AddTransient<CustomCommandFeature>();
        collection.AddTransient<BirthdayFeature>();
        collection.AddTransient<ReminderFeature>();
        collection.AddTransient<TwitchNotificationFeature>();
        collection.AddTransient<YoutubeAddPlaylistFeature>();
        collection.AddTransient<AudioPlayFeature>();
        collection.AddTransient<AudioRequestFeature>();

        //Services
        collection.AddSingleton<BrowserService>();
        collection.AddTransient<ITwitchAPI, TwitchAPI>();
        collection.AddTransient<ITwitchCLI, TwitchCLI>();
        collection.AddTransient<ISpotifyAPI, Services.SpotifyAPI>();
        collection.AddTransient<IYoutubeAPI, YoutubeAPI>();
        collection.AddTransient<IInstaLoader, InstaLoader>();
        collection.AddTransient<IWordOfTheDayService, WordOfTheDayService>();
        collection.AddTransient<IYoutubeDownloadService, YoutubeStreamService>();
        collection.AddTransient<ILastFmAPI, LastFmAPI>();
        collection.AddTransient<IMusicBrainzAPI, MusicBrainzAPI>();
        collection.AddTransient<ITwitterScraper, TwitterScraper>();
        collection.AddTransient<IKpopDbScraper, KpopDbScraper>();

        //Database Services
        collection.AddTransient<IServerService, ServerService>();
        collection.AddTransient<IServerChannelViewService, ServerChannelViewService>();
        collection.AddTransient<IGreetingService, GreetingService>();
        collection.AddTransient<ITwitchChannelService, TwitchChannelService>();
        collection.AddTransient<ICustomCommandService, CustomCommandService>();
        collection.AddTransient<IRoleService, RoleService>();
        collection.AddTransient<IKeywordService, KeywordService>();
        collection.AddTransient<IReminderService, ReminderService>();
        collection.AddTransient<IChannelService, ChannelService>();
        collection.AddTransient<IBirthdayService, BirthdayService>();
        collection.AddTransient<IIdolService, IdolService>();
        collection.AddTransient<IUserService, UserService>();
        collection.AddTransient<IIdolAliasService, IdolAliasService>();
        collection.AddTransient<IIdolGroupService, IdolGroupService>();
        collection.AddTransient<IUserIdolService, UserIdolService>();
        collection.AddTransient<IIdolImageService, IdolImageService>();
        collection.AddTransient<IUserIdolStatisticService, UserIdolStatisticService>();

        //Database Repositories
        collection.AddScoped<IServerRepository, ServerRepository>();
        collection.AddScoped<IServerChannelViewRepository, ServerChannelViewRepository>();
        collection.AddScoped<IGreetingRepository, GreetingRepository>();
        collection.AddScoped<ITwitchChannelRepository, TwitchChannelRepository>();
        collection.AddScoped<ICustomCommandRepository, CustomCommandRepository>();
        collection.AddScoped<IRoleRepository, RoleRepository>();
        collection.AddScoped<IKeywordRepository, KeywordRepository>();
        collection.AddScoped<IReminderRepository, ReminderRepository>();
        collection.AddScoped<IUserRepository, UserRepository>();
        collection.AddScoped<IChannelRepository, ChannelRepository>();
        collection.AddScoped<IChannelTypeRepository, ChannelTypeRepository>();
        collection.AddScoped<IBirthdayRepository, BirthdayRepository>();
        collection.AddScoped<IIdolRepository, IdolRepository>();
        collection.AddScoped<IIdolGroupRepository, IdolGroupRepository>();
        collection.AddScoped<IIdolAliasRepository, IdolAliasRepository>();
        collection.AddScoped<IIdolImageRepository, IdolImageRepository>();
        collection.AddScoped<IUserIdolStatisticRepository, UserIdolStatisticRepository>();

        return collection.BuildServiceProvider();
    }
}
