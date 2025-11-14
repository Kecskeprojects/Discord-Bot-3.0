using System.Collections.Generic;

namespace Discord_Bot.Resources;

public class WeeklyPollEditResource
{
    public int WeeklyPollId { get; set; }
    public bool IsActive { get; set; }
    public bool IsPinned { get; set; }
    public bool IsMultipleAnswer { get; set; }
    public int? OptionPresetId { get; set; }
    public long CloseInTimeSpanTicks { get; set; }
    public string RepeatOnDayOfWeek { get; set; }
    public string Name { get; set; }

    public List<WeeklyPollOptionResource> Options { get; set; }
}
