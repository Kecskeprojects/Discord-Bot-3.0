using AutoMapper;
using Discord_Bot.Database.Models;
using Discord_Bot.Resources;
using System.Linq;

namespace Discord_Bot.Core
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            //Provide all the Mapping Configuration
            CreateMap<Server, ServerResource>()
                .ForMember(dest => dest.SettingsChannels, opt => opt.MapFrom(sv =>
                        sv.Channels
                        .Select(ch => new ServerSettingsChannelResource(ch.DiscordId, 0))
                        .ToList()//Make server settings into lists, and then combine those lists to get a complete list?
                ));
        }
    }
}
