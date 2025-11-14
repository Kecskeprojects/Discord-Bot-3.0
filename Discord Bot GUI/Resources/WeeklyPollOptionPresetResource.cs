using System.Collections.Generic;

namespace Discord_Bot.Resources;

public class WeeklyPollOptionPresetResource
{
    public int WeeklyPollOptionPresetId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsSpecialPreset { get; set; }
    public bool IsActive { get; set; }

    public List<WeeklyPollOptionResource> Options { get; set; }
}
