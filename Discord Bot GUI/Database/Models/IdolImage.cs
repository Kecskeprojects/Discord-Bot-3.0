using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class IdolImage
{
    public int ImageId { get; set; }

    public int IdolId { get; set; }

    public string ImageUrl { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public virtual Idol Idol { get; set; }
}
