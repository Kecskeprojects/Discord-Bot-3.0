using AutoMapper;
using Discord_Bot.Database.Models;
using Discord_Bot.Resources;
using System.Linq;

namespace Discord_Bot
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            //Provide all the Mapping Configuration
            CreateMap<Server, ServerResource>()
                .ForMember(dest => dest.MusicChannel, opt => opt.MapFrom(sv => 
                        sv.Channels
                        .Where(ch => ch.ServerSettingChannels
                            .Where(sett => sett.ChannelType.Name == "MusicText")
                            .Select(sett => sett.ChannelId)
                            .Contains(ch.ChannelId)
                        )
                        .Select(ch => ulong.Parse(ch.DiscordId))
                        .ToArray()
                ));
        }
    }
}
