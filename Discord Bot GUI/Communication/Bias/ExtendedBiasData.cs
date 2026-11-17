using AngleSharp.Dom;
using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Communication.Bias;

public class ExtendedBiasData
{
    public ExtendedBiasData(IElement row)
    {
        string profileHref = Cell(row, ".column-profile>a").GetAttribute("href")
            ?? throw new InvalidOperationException("A dbkpop profile link has no href, the page layout may have changed");
        ProfileUrl = Uri.UnescapeDataString(profileHref.Trim());
        StageName = CellText(row, ".column-stage_name");
        FullName = CellText(row, ".column-full_name");
        KoreanFullName = CellText(row, ".column-korean_name");
        KoreanStageName = CellText(row, ".column-korean_stage_name");
        string dateString = CellText(row, ".column-dob");
        DateOfBirth = DateOnly.TryParseExact(dateString, "yyyy-MM-dd", out DateOnly date) ? date : null;
        GroupName = CellText(row, ".column-grp");
        string gender = CellText(row, ".column-gender");
        Gender = GenderChoiceEnumExtension.FromDatabaseString(gender);
    }

    public string ProfileUrl { get; private set; }
    public string StageName { get; private set; }
    public string FullName { get; private set; }
    public string KoreanFullName { get; private set; }
    public string KoreanStageName { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string GroupName { get; private set; }
    public GenderEnum? Gender { get; private set; }

    //Every row of the dbkpop table has these cells, a missing one means the page layout changed
    private static IElement Cell(IElement row, string selector)
    {
        return row.QuerySelector(selector)
            ?? throw new InvalidOperationException($"A dbkpop row has no '{selector}' cell, the page layout may have changed");
    }

    private static string CellText(IElement row, string selector)
    {
        return Uri.UnescapeDataString(Cell(row, selector).InnerHtml.Trim());
    }
}
