using System;

namespace Discord_Bot.Resources;
public class IdolExtendedResource
{
    public int IdolId { get; set; }

    public string Name { get; set; }

    public string GroupName { get; set; }

    public string ProfileUrl { get; set; }

    public string StageName { get; set; }

    public string FullName { get; set; }

    public string KoreanFullName { get; set; }

    public string KoreanStageName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? DebutDate { get; set; }

    public string Gender { get; set; }

    public string LatestImageUrl { get; set; }
}
