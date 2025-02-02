using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class WeeklyPoll
{
    public int WeeklyPollId { get; set; }

    public int ServerId { get; set; }

    public int ChannelId { get; set; }

    public int? RoleId { get; set; }

    public string Name { get; set; }

    public string Title { get; set; }

    public long CloseInTimeSpanTicks { get; set; }

    public string RepeatOnDayOfWeek { get; set; }

    public bool IsMultipleAnswer { get; set; }

    public int? OptionPresetId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public virtual Channel Channel { get; set; }

    public virtual WeeklyPollOptionPreset OptionPreset { get; set; }

    public virtual Role Role { get; set; }

    public virtual Server Server { get; set; }

    public virtual ICollection<WeeklyPollOption> WeeklyPollOptions { get; set; } = new List<WeeklyPollOption>();
}
