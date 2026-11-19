using DatabaseORM.Communication.Bias;
using DatabaseORM.Enums;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using DatabaseORM.Tools;
using Mapster;

namespace DatabaseORM.Mapping;

/// <summary>
/// Mapster registrations of the database layer: EF models to resources, and scraped data onto EF models.
/// Unlike AutoMapper, Mapster does not null-guard member chains or convert string ids implicitly,
/// so navigation-based and string → ulong mappings are written out explicitly.
/// </summary>
public class DatabaseMapperConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Same-type members are copied by reference, like AutoMapper did,
        // instead of being deep-cloned (which would recurse through EF navigation properties)
        _ = config.Default.ShallowCopyForSameType(true);

        //Model to Resource
        _ = config.NewConfig<Server, ServerResource>()
            .Map(dest => dest.DiscordId, src => ulong.Parse(src.DiscordId))
            .Map(dest => dest.RoleMessageDiscordId, src => string.IsNullOrEmpty(src.RoleMessageDiscordId) ? null : (ulong?) ulong.Parse(src.RoleMessageDiscordId))
            .Map(dest => dest.NotificationRoleDiscordId, src => src.NotificationRole != null ? (ulong?) ulong.Parse(src.NotificationRole.DiscordId) : null)
            .Map(dest => dest.NotificationRoleName, src => src.NotificationRole != null ? src.NotificationRole.RoleName : null)
            .Map(dest => dest.MuteRoleDiscordId, src => src.MuteRole != null ? (ulong?) ulong.Parse(src.MuteRole.DiscordId) : null)
            .Map(dest => dest.MuteRoleName, src => src.MuteRole != null ? src.MuteRole.RoleName : null)
            .Ignore(dest => dest.SettingsChannels); //ServerService fills it from the server's channels
        _ = config.NewConfig<Greeting, GreetingResource>();
        _ = config.NewConfig<TwitchChannel, TwitchChannelResource>()
            .Map(dest => dest.ServerDiscordId, src => src.Server != null ? ulong.Parse(src.Server.DiscordId) : 0)
            .Map(dest => dest.NotificationRoleDiscordId, src => src.Server != null && src.Server.NotificationRole != null ? (ulong?) ulong.Parse(src.Server.NotificationRole.DiscordId) : null)
            .Map(dest => dest.NotificationRoleName, src => src.Server != null && src.Server.NotificationRole != null ? src.Server.NotificationRole.RoleName : null);
        _ = config.NewConfig<CustomCommand, CustomCommandResource>();
        _ = config.NewConfig<Role, RoleResource>()
            .Map(dest => dest.DiscordId, src => ulong.Parse(src.DiscordId));
        _ = config.NewConfig<Reminder, ReminderResource>()
            .Map(dest => dest.UserDiscordId, src => src.User != null ? ulong.Parse(src.User.DiscordId) : 0);
        _ = config.NewConfig<Birthday, BirthdayResource>()
            .Map(dest => dest.UserDiscordId, src => src.User != null ? ulong.Parse(src.User.DiscordId) : 0)
            .Map(dest => dest.ServerDiscordId, src => src.Server != null ? ulong.Parse(src.Server.DiscordId) : 0);
        _ = config.NewConfig<Idol, IdolResource>()
            .Map(dest => dest.GroupName, src => src.Group != null ? src.Group.Name : "")
            .Map(dest => dest.GroupDebutDate, src => src.Group != null ? src.Group.DebutDate : null)
            .Map(dest => dest.CurrentImageUrl, src => src.IdolImages.OrderByDescending(img => img.CreatedOn).Select(img => img.ImageUrl).FirstOrDefault());
        _ = config.NewConfig<IdolAlias, IdolAliasResource>();
        _ = config.NewConfig<User, UserResource>()
            .Map(dest => dest.DiscordId, src => ulong.Parse(src.DiscordId))
            .Map(dest => dest.LastFmUsername, src => src.LastFmusername)
            .Ignore(dest => dest.Username); //The bot fills it with the member's name on the server (DiscordTools)
        _ = config.NewConfig<Idol, IdolExtendedResource>()
            .Map(dest => dest.GroupName, src => src.Group != null ? src.Group.Name : "")
            .Ignore(dest => dest.LatestImageUrl); //IdolService looks it up separately
        _ = config.NewConfig<IdolGroup, IdolGroupExtendedResource>();
        _ = config.NewConfig<Idol, IdolGameResource>()
            .Map(dest => dest.GroupFullName, src => src.Group != null && src.Group.FullName != null ? src.Group.FullName : "Soloist")
            .Map(dest => dest.GroupFullKoreanName, src => src.Group != null ? src.Group.FullKoreanName : null)
            .Map(dest => dest.LatestImageUrl, src => src.IdolImages.OrderByDescending(img => img.CreatedOn).Select(img => img.ImageUrl).FirstOrDefault());
        _ = config.NewConfig<User, UserBiasGameStatResource>()
            .Map(dest => dest.DiscordId, src => ulong.Parse(src.DiscordId))
            .Ignore(dest => dest.Stats);
        _ = config.NewConfig<ServerMutedUser, ServerMutedUserResource>()
            .Map(dest => dest.ServerDiscordId, src => src.Server != null ? src.Server.DiscordId : "")
            .Map(dest => dest.UserDiscordId, src => src.User != null ? src.User.DiscordId : "");
        _ = config.NewConfig<WeeklyPoll, WeeklyPollResource>()
            .Map(dest => dest.ServerDiscordId, src => src.Server != null ? ulong.Parse(src.Server.DiscordId) : 0)
            .Map(dest => dest.ChannelDiscordId, src => src.Channel != null ? (ulong?) ulong.Parse(src.Channel.DiscordId) : null)
            .Map(dest => dest.RoleDiscordId, src => src.Role != null ? (ulong?) ulong.Parse(src.Role.DiscordId) : null)
            .Map(dest => dest.Options, src => WeeklyPollTools.GetAnswerOptions(src, DateTime.UtcNow));
        _ = config.NewConfig<WeeklyPollOptionPreset, WeeklyPollOptionPresetResource>()
            .Map(dest => dest.Options, src => src.WeeklyPollOptions);
        _ = config.NewConfig<WeeklyPollOption, WeeklyPollOptionResource>()
            .Map(dest => dest.WeeklyPollId, src => src.WeeklyPollId ?? 0);
        _ = config.NewConfig<WeeklyPoll, WeeklyPollEditResource>()
            .Map(dest => dest.Options, src => src.WeeklyPollOptions);

        //Communication to Model
        _ = config.NewConfig<ExtendedBiasData, Idol>()
            .Map(dest => dest.Gender, src => src.Gender.HasValue ? src.Gender.Value.ToDatabaseFriendlyString() : null)
            .Map(dest => dest.ModifiedOn, src => DateTime.UtcNow);
        _ = config.NewConfig<ExtendedBiasData, IdolGroup>()
            .Ignore(dest => dest.Name);
        _ = config.NewConfig<AdditionalIdolData, IdolGroup>()
            .Map(dest => dest.FullName, src => src.GroupFullName)
            .Map(dest => dest.FullKoreanName, src => src.GroupFullKoreanName)
            .Map(dest => dest.ModifiedOn, src => DateTime.UtcNow);
    }
}
