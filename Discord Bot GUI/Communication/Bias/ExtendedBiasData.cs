using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Communication.Bias;

//One idol row of the dbkpop table, filled by KpopDbScraper
public class ExtendedBiasData
{
    public string ProfileUrl { get; set; } = "";
    public string StageName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string KoreanFullName { get; set; } = "";
    public string KoreanStageName { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public string GroupName { get; set; } = "";
    public GenderEnum? Gender { get; set; }
}
