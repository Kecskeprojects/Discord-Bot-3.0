using System;

namespace Discord_Bot.Resources;

public class IdolResource
{
    public int IdolId { get; set; }

    public string Name { get; set; }

    public string ProfileUrl { get; set; }

    public string GroupName { get; set; }

    public string CurrentImageUrl { get; set; }

    public DateOnly? GroupDebutDate { get; set; }
}
