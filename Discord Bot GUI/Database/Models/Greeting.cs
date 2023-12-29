using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Greeting
{
    public int GreetingId { get; set; }

    public string Url { get; set; }

    public DateTime CreatedOn { get; set; }
}
