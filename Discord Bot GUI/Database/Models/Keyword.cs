using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Keyword
{
    public int KeywordId { get; set; }

    public int ServerId { get; set; }

    public string Trigger { get; set; }

    public string Response { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Server Server { get; set; }
}
