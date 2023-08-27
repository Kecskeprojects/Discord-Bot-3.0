using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database;
using Discord_Bot.Database.DBRepositories;
using Discord_Bot.Database.DBServices;
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
                        DefaultRunMode = Discord.Interactions.RunMode.Async
                    });

            CommandService commands = new(new CommandServiceConfig()
            {
                DefaultRunMode = Discord.Commands.RunMode.Async
            });
            Config.Config config = new();

            IServiceCollection collection = new ServiceCollection();

            //Singletons
            collection.AddSingleton(client);
            collection.AddSingleton(interactions);
            collection.AddSingleton(commands);
            collection.AddSingleton(new Logging());
            collection.AddSingleton(config);
            collection.AddSingleton(x => new CoreLogic(x.GetService<Logging>()));
            collection.AddSingleton(new Cache());

            collection.AddDbContext<MainDbContext>(options => options.UseSqlServer(config.SqlConnectionString));

            collection.AddAutoMapper(x => x.AddProfile<MapperConfig>());

            collection.AddTransient(typeof(MainWindow));

            //API
            collection.AddScoped<IYoutubeAPI, YoutubeAPI>();

            //Database
            collection.AddScoped<IServerRepository, ServerRepository>();
            collection.AddScoped<IServerService, ServerService>();
            collection.AddScoped<IServerChannelViewRepository, ServerChannelViewRepository>();
            collection.AddScoped<IGreetingRepository, GreetingRepository>();
            collection.AddScoped<IGreetingService, GreetingService>();

            return collection.BuildServiceProvider();
        }
    }
}
