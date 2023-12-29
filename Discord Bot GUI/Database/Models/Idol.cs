using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Idol
{
    public int IdolId { get; set; }

    public string Name { get; set; }

    public int GroupId { get; set; }

    public string ProfileUrl { get; set; }

    public string StageName { get; set; }

    public string FullName { get; set; }

    public string KoreanFullName { get; set; }

    public string KoreanStageName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string Gender { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public virtual IdolGroup Group { get; set; }

    public virtual ICollection<IdolAlias> IdolAliases { get; set; } = new List<IdolAlias>();

    public virtual ICollection<IdolImage> IdolImages { get; set; } = new List<IdolImage>();

    public virtual ICollection<UserIdolStatistic> UserIdolStatistics { get; set; } = new List<UserIdolStatistic>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
