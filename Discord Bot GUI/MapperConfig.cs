using AutoMapper;
using Discord_Bot.Database.Models;
using Discord_Bot.Resources;

namespace Discord_Bot
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            //Provide all the Mapping Configuration
            CreateMap<Server, ServerResource>();
        }
    }
}
