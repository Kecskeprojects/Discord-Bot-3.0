using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class IdolGroup
{
    public int GroupId { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Idol> Idols { get; set; } = new List<Idol>();
}
