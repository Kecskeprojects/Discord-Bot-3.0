using AngleSharp.Dom;
using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Communication.Bias
{
    public class ExtendedBiasData
    {
        public ExtendedBiasData(IElement row)
        {
            ProfileUrl = Uri.UnescapeDataString(row.QuerySelector(".column-profile>a").GetAttribute("href").Trim());
            StageName = Uri.UnescapeDataString(row.QuerySelector(".column-stage_name").InnerHtml.Trim());
            FullName = Uri.UnescapeDataString(row.QuerySelector(".column-full_name").InnerHtml.Trim());
            KoreanFullName = Uri.UnescapeDataString(row.QuerySelector(".column-korean_name").InnerHtml.Trim());
            KoreanStageName = Uri.UnescapeDataString(row.QuerySelector(".column-korean_stage_name").InnerHtml.Trim());
            string dateString = Uri.UnescapeDataString(row.QuerySelector(".column-dob").InnerHtml.Trim());
            DateOfBirth = DateOnly.TryParseExact(dateString, "yyyy-MM-dd", out DateOnly date) ? date : null;
            GroupName = Uri.UnescapeDataString(row.QuerySelector(".column-grp").InnerHtml.Trim());
            string gender = Uri.UnescapeDataString(row.QuerySelector(".column-gender").InnerHtml.Trim());
            Gender = gender == GenderType.Male ? GenderType.Male : gender == GenderType.Female ? GenderType.Female : null;
        }

        public string ProfileUrl { get; private set; }
        public string StageName { get; private set; }
        public string FullName { get; private set; }
        public string KoreanFullName { get; private set; }
        public string KoreanStageName { get; private set; }
        public DateOnly? DateOfBirth { get; private set; }
        public string GroupName { get; private set; }
        public GenderType Gender { get; private set; }
    }
}
