using DatabaseORM.Mapping;
using Mapster;
using MapsterMapper;

namespace Discord_Bot.Tests.Common.Tools;

//A real IMapper over the given registrations, compiled the way Startup compiles the bot's
public static class TestMapper
{
    //The database layer's mappings, for the DB service tests; a compiled mapper can be shared by tests running in parallel
    public static IMapper Database { get; } = Create(new DatabaseMapperConfig());

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
