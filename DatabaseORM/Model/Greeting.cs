using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class Greeting
{
    public int GreetingId { get; set; }

    public string Url { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
