using DatabaseORM.Caching;
using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Repository;
using DatabaseORM.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseORM;

/// <summary>
/// Registers the database layer: the DbContext, the server cache, the services and the repositories.
/// The application registers the Mapster mappings (<see cref="Mapping.DatabaseMapperConfig"/>) and the
/// <see cref="Interface.IBotLogger"/> implementation itself, because its own code shares them.
/// </summary>
public static class DatabaseStartup
{
    /// <param name="services">The application's service collection.</param>
    /// <param name="connectionString">
    /// Read from the application's services whenever a DbContext is configured (once per scope), so a connection string
    /// edited in the configuration applies without a restart, and replacing the configuration in a test replaces it too.
    /// </param>
    public static void ConfigureDatabaseServices(IServiceCollection services, Func<IServiceProvider, string> connectionString)
    {
        _ = services.AddDbContext<MainDbContext>((provider, options) => options.UseSqlServer(connectionString(provider)));
        _ = services.AddSingleton<ServerCache>();

        //Services
        _ = services.AddTransient<IServerService, ServerService>();
        _ = services.AddTransient<IGreetingService, GreetingService>();
        _ = services.AddTransient<ITwitchChannelService, TwitchChannelService>();
        _ = services.AddTransient<ICustomCommandService, CustomCommandService>();
        _ = services.AddTransient<IRoleService, RoleService>();
        _ = services.AddTransient<IReminderService, ReminderService>();
        _ = services.AddTransient<IChannelService, ChannelService>();
        _ = services.AddTransient<IBirthdayService, BirthdayService>();
        _ = services.AddTransient<IIdolService, IdolService>();
        _ = services.AddTransient<IUserService, UserService>();
        _ = services.AddTransient<IIdolAliasService, IdolAliasService>();
        _ = services.AddTransient<IIdolGroupService, IdolGroupService>();
        _ = services.AddTransient<IUserIdolService, UserIdolService>();
        _ = services.AddTransient<IIdolImageService, IdolImageService>();
        _ = services.AddTransient<IUserIdolStatisticService, UserIdolStatisticService>();
        _ = services.AddTransient<IWeeklyPollOptionPresetService, WeeklyPollOptionPresetService>();
        _ = services.AddTransient<IWeeklyPollOptionService, WeeklyPollOptionService>();
        _ = services.AddTransient<IWeeklyPollService, WeeklyPollService>();
        _ = services.AddTransient<IServerMutedUserService, ServerMutedUserService>();

        //Repositories
        _ = services.AddScoped<IServerRepository, ServerRepository>();
        _ = services.AddScoped<IGreetingRepository, GreetingRepository>();
        _ = services.AddScoped<ITwitchChannelRepository, TwitchChannelRepository>();
        _ = services.AddScoped<ICustomCommandRepository, CustomCommandRepository>();
        _ = services.AddScoped<IRoleRepository, RoleRepository>();
        _ = services.AddScoped<IReminderRepository, ReminderRepository>();
        _ = services.AddScoped<IUserRepository, UserRepository>();
        _ = services.AddScoped<IChannelRepository, ChannelRepository>();
        _ = services.AddScoped<IChannelTypeRepository, ChannelTypeRepository>();
        _ = services.AddScoped<IBirthdayRepository, BirthdayRepository>();
        _ = services.AddScoped<IIdolRepository, IdolRepository>();
        _ = services.AddScoped<IIdolGroupRepository, IdolGroupRepository>();
        _ = services.AddScoped<IIdolAliasRepository, IdolAliasRepository>();
        _ = services.AddScoped<IIdolImageRepository, IdolImageRepository>();
        _ = services.AddScoped<IUserIdolStatisticRepository, UserIdolStatisticRepository>();
        _ = services.AddScoped<IWeeklyPollOptionPresetRepository, WeeklyPollOptionPresetRepository>();
        _ = services.AddScoped<IWeeklyPollOptionRepository, WeeklyPollOptionRepository>();
        _ = services.AddScoped<IWeeklyPollRepository, WeeklyPollRepository>();
        _ = services.AddScoped<IServerMutedUserRepository, ServerMutedUserRepository>();
    }
}
