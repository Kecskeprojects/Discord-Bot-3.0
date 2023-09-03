using AutoMapper;
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
                .ForMember(dest => dest.DiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.DiscordId)));
            CreateMap<IGrouping<int?, ServerChannelView>, KeyValuePair<ChannelTypeEnum, List<ulong>>>()
            .ConstructUsing(scv => new KeyValuePair<ChannelTypeEnum, List<ulong>>(
                scv.Key != null ? (ChannelTypeEnum)scv.Key : ChannelTypeEnum.None,
                scv.Select(x => ulong.Parse(x.ChannelDiscordId)).ToList()));
            CreateMap<TwitchChannel, TwitchChannelResource>();
            CreateMap<Greeting, GreetingResource>();
            CreateMap<TwitchChannel, TwitchChannelResource>()
                .ForMember(dest => dest.ServerDiscordId, opt => opt.MapFrom(scv => ulong.Parse(scv.Server.DiscordId)));
            CreateMap<CustomCommand, CustomCommandResource>();
            CreateMap<Role, RoleResource>()
                .ForMember(dest => dest.DiscordId, opt => opt.MapFrom(r => ulong.Parse(r.DiscordId)));
            CreateMap<Keyword, KeywordResource>();
            CreateMap<Reminder, ReminderResource>()
                .ForMember(dest => dest.UserDiscordId, opt => opt.MapFrom(r => ulong.Parse(r.User.DiscordId)));
        }
    }
}
