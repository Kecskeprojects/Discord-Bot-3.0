using System;
using System.Collections.Generic;

namespace Discord_Bot.Database.Models;

public partial class Embed
{
    public int EmbedId { get; set; }

    public int EmbedGroupId { get; set; }

    public int Order { get; set; }

    public int ContentType { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public string ImageUrl { get; set; }

    public string EmbedContent { get; set; }

    public virtual EmbedGroup EmbedGroup { get; set; }
}
