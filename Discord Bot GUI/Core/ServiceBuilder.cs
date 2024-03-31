using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Commands.Communication;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database;
using Discord_Bot.Database.DBRepositories;
using Discord_Bot.Database.DBServices;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Discord_Bot.Core
{
    public static class ServiceBuilder
    {
        public static IServiceProvider BuildService()
        {
            DiscordSocketClient client = new(new DiscordSocketConfig()
            {
                //GatewayIntents.GuildPresences | GatewayIntents.GuildScheduledEvents | GatewayIntents.GuildInvites  NOT USED GATEWAY INTENTS
                GatewayIntents = GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageTyping |
                                 GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations | GatewayIntents.GuildMembers |
                                 GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageTyping |
                                 GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildWebhooks | GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Info
            });
            InteractionService interactions = new(client,
                new InteractionServiceConfig()
                {
                    DefaultRunMode = Discord.Interactions.RunMode.Async,
                    UseCompiledLambda = true,
                });

            CommandService commands = new(new CommandServiceConfig()
            {
                DefaultRunMode = Discord.Commands.RunMode.Async
            });
            Config.Config config = new();

            IServiceCollection collection = new ServiceCollection();

            //Core
            collection.AddSingleton(client);
            collection.AddSingleton(interactions);
            collection.AddSingleton(commands);
            collection.AddSingleton(config);
            collection.AddSingleton(new Logging());
            collection.AddSingleton(new Cache());

            collection.AddDbContext<MainDbContext>(options => options.UseSqlServer(config.SqlConnectionString));

            collection.AddAutoMapper(x => x.AddProfile<MapperConfig>());

            collection.AddTransient<ICoreLogic, CoreLogic>();
            collection.AddTransient(typeof(MainWindow));

            //Services
            collection.AddTransient<ITwitchAPI, TwitchAPI>();
            collection.AddTransient<ISpotifyAPI, Services.SpotifyAPI>();
            collection.AddTransient<IYoutubeAPI, YoutubeAPI>();
            collection.AddTransient<IPictureHandler, PictureHandler>();
            collection.AddTransient<IInstaLoader, InstaLoader>();
            collection.AddTransient<IWordOfTheDayService, WordOfTheDayService>();
            collection.AddTransient<IAudioService, AudioService>();
            collection.AddTransient<IYoutubeDownloadService, YoutubeDownloadService>();
            collection.AddTransient<ILastFmAPI, LastFmAPI>();
            collection.AddTransient<IMusicBrainzAPI, MusicBrainzAPI>();
            collection.AddTransient<ITwitterScraper, TwitterScraper>();
            collection.AddTransient<IBiasDatabaseService, BiasDatabaseService>();
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

            //Commands
            collection.AddTransient<IServiceToDiscordCommunication, ServiceDiscordCommunication>();
            collection.AddTransient<ICoreToDiscordCommunication, CoreDiscordCommunication>();

            return collection.BuildServiceProvider();
        }
    }
}
