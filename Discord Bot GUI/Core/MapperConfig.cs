﻿using AutoMapper;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Core
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            //Provide all the Mapping Configuration
            CreateMap<Server, ServerResource>()
                .ForMember(dest => dest.DiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.DiscordId)))
                .ForMember(dest => dest.RoleMessageDiscordId, opt => opt.MapFrom(scv => scv.RoleMessageDiscordId))
                .ForMember(dest => dest.NotificationRoleDiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.NotificationRole.DiscordId)))
                .ForMember(dest => dest.NotificationRoleName, opt => opt.MapFrom(scv => scv.NotificationRole.RoleName));
            CreateMap<IGrouping<int?, ServerChannelView>, KeyValuePair<ChannelTypeEnum, List<ulong>>>()
            .ConstructUsing(scv => new KeyValuePair<ChannelTypeEnum, List<ulong>>(
                scv.Key != null ? (ChannelTypeEnum)scv.Key : ChannelTypeEnum.None,
                scv.Select(x => ulong.Parse(x.ChannelDiscordId)).ToList()));
            CreateMap<TwitchChannel, TwitchChannelResource>();
            CreateMap<Greeting, GreetingResource>();
            CreateMap<TwitchChannel, TwitchChannelResource>()
                .ForMember(dest => dest.ServerDiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.Server.DiscordId)))
                .ForMember(dest => dest.NotificationRoleDiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.Server.NotificationRole.DiscordId)))
                .ForMember(dest => dest.NotificationRoleName, opt => opt.MapFrom(scv => scv.Server.NotificationRole.RoleName));
            CreateMap<CustomCommand, CustomCommandResource>();
            CreateMap<Role, RoleResource>()
                .ForMember(dest => dest.DiscordId, opt => opt.MapFrom(r => ulong.Parse(r.DiscordId)));
            CreateMap<Keyword, KeywordResource>();
            CreateMap<Reminder, ReminderResource>()
                .ForMember(dest => dest.UserDiscordId, opt => opt.MapFrom(r => ulong.Parse(r.User.DiscordId)));
            CreateMap<Birthday, BirthdayResource>()
                .ForMember(dest => dest.UserDiscordId, opt => opt.MapFrom(r => ulong.Parse(r.User.DiscordId)))
                .ForMember(dest => dest.ServerDiscordId, opt => opt.MapFrom(r => ulong.Parse(r.Server.DiscordId)));
            CreateMap<Idol, IdolResource>()
                .ForMember(dest => dest.IdolId, opt => opt.MapFrom(i => i.IdolId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(i => i.Name));
            CreateMap<IdolGroup, IdolGroupResource>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(ig => ig.GroupId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(ig => ig.Name));
            CreateMap<IdolAlias, IdolAliasResource>()
                .ForMember(dest => dest.IdolAliasId, opt => opt.MapFrom(ia => ia.IdolAliasId))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(ia => ia.Alias));
            CreateMap<User, UserResource>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(u => u.UserId))
                .ForMember(dest => dest.DiscordId, opt => opt.MapFrom(u => ulong.Parse(u.DiscordId)))
                .ForMember(dest => dest.LastFmUsername, opt => opt.MapFrom(u => u.LastFmusername));
        }
    }
}
