using System;

namespace Discord_Bot.Communication
{
    public class AdditionalIdolData
    {
        public AdditionalIdolData() 
        {
        }

        public DateOnly? DebutDate { get; set; }
        public string GroupFullName { get; set; }
        public string GroupFullKoreanName { get; set; }
        public string ImageUrl { get; set; }
    }
}
