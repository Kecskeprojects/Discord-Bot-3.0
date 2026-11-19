using Mapster;
using MapsterMapper;

namespace Discord_Bot.Tests.Common.Tools;

//A real IMapper over the given registrations, compiled the way Startup compiles the bot's
public static class TestMapper
{
    public static IMapper Create(params IRegister[] registers)
    {
        TypeAdapterConfig config = new();
        foreach (IRegister register in registers)
        {
            register.Register(config);
        }

        config.Compile();
        return new Mapper(config);
    }
}
