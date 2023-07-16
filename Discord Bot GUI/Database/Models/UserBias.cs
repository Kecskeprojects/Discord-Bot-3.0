using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class UserBias
{
    public int UserBiasId { get; set; }

    public int UserId { get; set; }

    public int IdolId { get; set; }

    public virtual Idol Idol { get; set; }

    public virtual User User { get; set; }
}
