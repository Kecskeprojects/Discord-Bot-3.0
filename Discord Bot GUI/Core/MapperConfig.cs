using AutoMapper;
using Discord;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Database.Models;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Linq;

namespace Discord_Bot.Core;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        //Model to Resource
        _ = CreateMap<Server, ServerResource>()
            .ForMember(dest => dest.DiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.DiscordId)))
            .ForMember(dest => dest.RoleMessageDiscordId, opt => opt.MapFrom(scv => scv.RoleMessageDiscordId))
            .ForMember(dest => dest.NotificationRoleDiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.NotificationRole.DiscordId)))
            .ForMember(dest => dest.NotificationRoleName, opt => opt.MapFrom(scv => scv.NotificationRole.RoleName))
            .ForMember(dest => dest.MuteRoleDiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.MuteRole.DiscordId)))
            .ForMember(dest => dest.MuteRoleName, opt => opt.MapFrom(scv => scv.MuteRole.RoleName));
        _ = CreateMap<TwitchChannel, TwitchChannelResource>();
        _ = CreateMap<Greeting, GreetingResource>();
        _ = CreateMap<TwitchChannel, TwitchChannelResource>()
            .ForMember(dest => dest.ServerDiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.Server.DiscordId)))
            .ForMember(dest => dest.NotificationRoleDiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.Server.NotificationRole.DiscordId)))
            .ForMember(dest => dest.NotificationRoleName, opt => opt.MapFrom(scv => scv.Server.NotificationRole.RoleName));
        _ = CreateMap<CustomCommand, CustomCommandResource>();
        _ = CreateMap<Role, RoleResource>()
            .ForMember(dest => dest.DiscordId, opt => opt.MapFrom(r => ulong.Parse(r.DiscordId)));
        _ = CreateMap<Reminder, ReminderResource>()
            .ForMember(dest => dest.UserDiscordId, opt => opt.MapFrom(r => ulong.Parse(r.User.DiscordId)));
        _ = CreateMap<Birthday, BirthdayResource>()
            .ForMember(dest => dest.UserDiscordId, opt => opt.MapFrom(r => ulong.Parse(r.User.DiscordId)))
            .ForMember(dest => dest.ServerDiscordId, opt => opt.MapFrom(r => ulong.Parse(r.Server.DiscordId)));
        _ = CreateMap<Idol, IdolResource>()
            .ForMember(dest => dest.IdolId, opt => opt.MapFrom(i => i.IdolId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(i => i.Name))
            .ForMember(dest => dest.CurrentImageUrl, opt => opt.MapFrom(i => i.IdolImages.Count > 0 ? i.IdolImages.OrderByDescending(img => img.CreatedOn).FirstOrDefault().ImageUrl : null));
        _ = CreateMap<IdolGroup, IdolGroupResource>()
            .ForMember(dest => dest.GroupId, opt => opt.MapFrom(ig => ig.GroupId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(ig => ig.Name));
        _ = CreateMap<IdolAlias, IdolAliasResource>()
            .ForMember(dest => dest.IdolAliasId, opt => opt.MapFrom(ia => ia.IdolAliasId))
            .ForMember(dest => dest.Alias, opt => opt.MapFrom(ia => ia.Alias));
        _ = CreateMap<User, UserResource>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(u => u.UserId))
            .ForMember(dest => dest.DiscordId, opt => opt.MapFrom(u => ulong.Parse(u.DiscordId)))
            .ForMember(dest => dest.LastFmUsername, opt => opt.MapFrom(u => u.LastFmusername));
        _ = CreateMap<Idol, IdolExtendedResource>();
        _ = CreateMap<IdolGroup, IdolGroupExtendedResource>();
        _ = CreateMap<Idol, IdolGameResource>()
            .ForMember(dest => dest.GroupFullName, opt => opt.MapFrom(i => i.Group.FullName ?? "Soloist"))
            .ForMember(dest => dest.LatestImageUrl, opt => opt.MapFrom(i => i.IdolImages.OrderByDescending(x => x.CreatedOn).First().ImageUrl));
        _ = CreateMap<User, UserBiasGameStatResource>()
            .ForMember(dest => dest.Stats, opt => opt.Ignore());
        _ = CreateMap<ServerMutedUser, ServerMutedUserResource>();
        _ = CreateMap<WeeklyPoll, WeeklyPollResource>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(x => PollTools.GetAnswerOptions(x)));
        _ = CreateMap<WeeklyPollOptionPreset, WeeklyPollOptionPresetResource>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(x => x.WeeklyPollOptions));
        _ = CreateMap<WeeklyPollOption, WeeklyPollOptionResource>();
        _ = CreateMap<WeeklyPoll, WeeklyPollEditResource>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(x => x.WeeklyPollOptions));

        //Communication to Model
        _ = CreateMap<ExtendedBiasData, Idol>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(e => e.Gender))
            .ForMember(dest => dest.ModifiedOn, opt => opt.MapFrom(x => DateTime.UtcNow));
        _ = CreateMap<ExtendedBiasData, IdolGroup>()
            .ForMember(dest => dest.Name, opt => opt.Ignore());
        _ = CreateMap<AdditionalIdolData, IdolGroup>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(e => e.GroupFullName))
            .ForMember(dest => dest.FullKoreanName, opt => opt.MapFrom(e => e.GroupFullKoreanName))
            .ForMember(dest => dest.DebutDate, opt => opt.MapFrom(e => e.DebutDate))
            .ForMember(dest => dest.ModifiedOn, opt => opt.MapFrom(x => DateTime.UtcNow));

        //Other
        _ = CreateMap<WeeklyPollResource, PollProperties>()
            .ForMember(dest => dest.Answers, opt => opt.MapFrom(x => x.Options.Select(PollTools.CreateTitle)))
            .ForMember(dest => dest.AllowMultiselect, opt => opt.MapFrom(x => x.IsMultipleAnswer))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(x => PollTools.GetDuration(x.CloseInTimeSpanTicks)))
            .ForMember(dest => dest.LayoutType, opt => opt.MapFrom(x => PollLayout.Default))
            .ForMember(dest => dest.Question, opt => opt.MapFrom(x => PollTools.CreateTitle(x.Title)));
    }
}
