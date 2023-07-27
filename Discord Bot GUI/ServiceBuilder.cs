using Discord_Bot.Assets;
using Discord_Bot.Database;
using Discord_Bot.Database.DBRepositories;
using Discord_Bot.Database.DBServices;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Discord_Bot
{
    public static class ServiceBuilder
    {
        public static IServiceProvider BuildService(IServiceCollection service, Config config)
        {
            service.AddDbContext<MainDbContext>(options => options.UseSqlServer(config.SqlConnectionString));
            service.AddAutoMapper(x => x.AddProfile<MapperConfig>());
            service.AddTransient(typeof(MainWindow));
            service.AddScoped<IServerRepository, ServerRepository>();
            service.AddScoped<IServerService, ServerService>();

            return service.BuildServiceProvider();
        }
    }
}
