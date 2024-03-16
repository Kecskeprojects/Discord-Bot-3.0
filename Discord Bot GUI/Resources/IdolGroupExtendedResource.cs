using System;

namespace Discord_Bot.Resources
{
    public class IdolGroupExtendedResource
    {
        public int GroupId { get; set; }

        public string Name { get; set; }

        public DateOnly? DebutDate { get; set; }

        public string FullName { get; set; }

        public string FullKoreanName { get; set; }
    }
}
