using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tools;
using Mapster;

namespace Discord_Bot.Core;

/// <summary>
/// The bot's Mapster registrations: resources to Discord types.
/// The database layer's own mappings are in DatabaseMapperConfig, registered next to this one in Startup.
/// </summary>
public class MapperConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        _ = config.NewConfig<WeeklyPollResource, PollProperties>()
            .MapWith(src => new PollProperties
            {
                Question = PollTools.CreateTitle(src.Title),
                Answers = src.Options.Select(option => PollTools.CreateTitle(option)).ToList(),
                AllowMultiselect = src.IsMultipleAnswer,
                Duration = PollTools.GetDuration(src.CloseInTimeSpanTicks),
                LayoutType = PollLayout.Default
            });
    }
}
