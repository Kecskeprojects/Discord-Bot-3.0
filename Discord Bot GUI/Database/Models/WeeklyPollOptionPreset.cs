using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class WeeklyPollOptionPreset
{
    public int WeeklyPollOptionPresetId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsSpecialPreset { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public virtual ICollection<WeeklyPollOption> WeeklyPollOptions { get; set; } = new List<WeeklyPollOption>();

    public virtual ICollection<WeeklyPoll> WeeklyPolls { get; set; } = new List<WeeklyPoll>();
}
