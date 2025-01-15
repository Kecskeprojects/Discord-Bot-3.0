using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class WeeklyPollOptionPreset
{
    public int WeeklyPollOptionPresetId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsSpecialPreset { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
