using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class WeeklyPollOption
{
    public int WeeklyPollOptionId { get; set; }

    public int? WeeklyPollId { get; set; }

    public int? WeeklyPollOptionPresetId { get; set; }

    public byte OrderNumber { get; set; }

    public string Title { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public virtual WeeklyPoll WeeklyPoll { get; set; }

    public virtual WeeklyPollOptionPreset WeeklyPollOptionPreset { get; set; }
}
