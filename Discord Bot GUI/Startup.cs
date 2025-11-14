using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
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
using Discord_Bot.Processors.EmbedProcessors.LastFm;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Services;
using Discord_Bot.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Discord_Bot;

public static class Startup
{
    public static IServiceProvider Run()
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
        _ = collection.AddSingleton(client);
        _ = collection.AddSingleton(interactions);
        _ = collection.AddSingleton(commands);
        _ = collection.AddTransient<Config>(); //Configuration can be edited without stopping the program
        _ = collection.AddSingleton(new BotLogger());
        _ = collection.AddSingleton(new ServerCache());

        Config config = new();
        _ = collection.AddDbContext<MainDbContext>(options => options.UseSqlServer(config.SqlConnectionString));

        _ = collection.AddAutoMapper(x => x.AddProfile<MapperConfig>());

        _ = collection.AddSingleton<BotMain>();
        _ = collection.AddSingleton<InteractionHandler>();
        _ = collection.AddSingleton<CommandHandler>();
        _ = collection.AddSingleton<BotWindow>();

        //Processors
        _ = collection.AddTransient<WhoKnowsImageProcessor>();
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
        _ = collection.AddSingleton<BrowserService>();
        _ = collection.AddTransient<ITwitchAPI, TwitchAPI>();
        _ = collection.AddTransient<ITwitchCLI, TwitchCLI>();
        _ = collection.AddTransient<ISpotifyAPI, Services.SpotifyAPI>();
        _ = collection.AddTransient<IYoutubeAPI, YoutubeAPI>();
        //collection.AddTransient<IInstaLoader, InstaLoader>();
        _ = collection.AddTransient<IInstaScraper, InstaScraper>();
        _ = collection.AddTransient<IWordOfTheDayService, WordOfTheDayService>();
        _ = collection.AddTransient<IYoutubeDownloadService, YoutubeStreamService>();
        _ = collection.AddTransient<ILastFmAPI, LastFmAPI>();
        _ = collection.AddTransient<IMusicBrainzAPI, MusicBrainzAPI>();
        _ = collection.AddTransient<ITwitterScraper, TwitterScraper>();
        _ = collection.AddTransient<IKpopDbScraper, KpopDbScraper>();

        //Database Services
        _ = collection.AddTransient<IServerService, ServerService>();
        _ = collection.AddTransient<IGreetingService, GreetingService>();
        _ = collection.AddTransient<ITwitchChannelService, TwitchChannelService>();
        _ = collection.AddTransient<ICustomCommandService, CustomCommandService>();
        _ = collection.AddTransient<IRoleService, RoleService>();
        _ = collection.AddTransient<IReminderService, ReminderService>();
        _ = collection.AddTransient<IChannelService, ChannelService>();
        _ = collection.AddTransient<IBirthdayService, BirthdayService>();
        _ = collection.AddTransient<IIdolService, IdolService>();
        _ = collection.AddTransient<IUserService, UserService>();
        _ = collection.AddTransient<IIdolAliasService, IdolAliasService>();
        _ = collection.AddTransient<IIdolGroupService, IdolGroupService>();
        _ = collection.AddTransient<IUserIdolService, UserIdolService>();
        _ = collection.AddTransient<IIdolImageService, IdolImageService>();
        _ = collection.AddTransient<IUserIdolStatisticService, UserIdolStatisticService>();
        _ = collection.AddTransient<IWeeklyPollOptionPresetService, WeeklyPollOptionPresetService>();
        _ = collection.AddTransient<IWeeklyPollOptionService, WeeklyPollOptionService>();
        _ = collection.AddTransient<IWeeklyPollService, WeeklyPollService>();
        _ = collection.AddTransient<IServerMutedUserService, ServerMutedUserService>();

        //Database Repositories
        _ = collection.AddScoped<IServerRepository, ServerRepository>();
        _ = collection.AddScoped<IGreetingRepository, GreetingRepository>();
        _ = collection.AddScoped<ITwitchChannelRepository, TwitchChannelRepository>();
        _ = collection.AddScoped<ICustomCommandRepository, CustomCommandRepository>();
        _ = collection.AddScoped<IRoleRepository, RoleRepository>();
        _ = collection.AddScoped<IReminderRepository, ReminderRepository>();
        _ = collection.AddScoped<IUserRepository, UserRepository>();
        _ = collection.AddScoped<IChannelRepository, ChannelRepository>();
        _ = collection.AddScoped<IChannelTypeRepository, ChannelTypeRepository>();
        _ = collection.AddScoped<IBirthdayRepository, BirthdayRepository>();
        _ = collection.AddScoped<IIdolRepository, IdolRepository>();
        _ = collection.AddScoped<IIdolGroupRepository, IdolGroupRepository>();
        _ = collection.AddScoped<IIdolAliasRepository, IdolAliasRepository>();
        _ = collection.AddScoped<IIdolImageRepository, IdolImageRepository>();
        _ = collection.AddScoped<IUserIdolStatisticRepository, UserIdolStatisticRepository>();
        _ = collection.AddScoped<IWeeklyPollOptionPresetRepository, WeeklyPollOptionPresetRepository>();
        _ = collection.AddScoped<IWeeklyPollOptionRepository, WeeklyPollOptionRepository>();
        _ = collection.AddScoped<IWeeklyPollRepository, WeeklyPollRepository>();
        _ = collection.AddScoped<IServerMutedUserRepository, ServerMutedUserRepository>();

        _ = collection.AddLogging();

        return collection.BuildServiceProvider();
    }
}
