using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class CustomCommand
{
    public int CommandId { get; set; }

    public int ServerId { get; set; }

    public string Command { get; set; }

    public string Url { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; }
}
