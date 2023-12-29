using AngleSharp.Dom;
using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Communication
{
    public class ExtendedBiasData
    {
        public ExtendedBiasData(IElement row)
        {
            ProfileUrl = row.QuerySelector(".column-profile>a").GetAttribute("href").Trim();
            StageName = row.QuerySelector(".column-stage_name").InnerHtml.Trim();
            FullName = row.QuerySelector(".column-full_name").InnerHtml.Trim();
            KoreanFullName = row.QuerySelector(".column-korean_name").InnerHtml.Trim();
            KoreanStageName = row.QuerySelector(".column-korean_stage_name").InnerHtml.Trim();
            string dateString = row.QuerySelector(".column-dob").InnerHtml.Trim();
            DateOfBirth = DateOnly.TryParseExact(dateString, "yyyy-MM-dd", out DateOnly date) ? date : null;
            GroupName = row.QuerySelector(".column-grp").InnerHtml.Trim();
            string gender = row.QuerySelector(".column-gender").InnerHtml.Trim();
            Gender = gender == GenderType.Male.Value ? GenderType.Male : gender == GenderType.Female.Value ? GenderType.Female : null;
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
