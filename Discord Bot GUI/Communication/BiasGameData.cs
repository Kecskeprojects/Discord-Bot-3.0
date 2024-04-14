using Discord;
using Discord_Bot.Enums;
using System;
using System.Collections.Generic;

namespace Discord_Bot.Communication
{
    public class BiasGameData(ulong userId)
    {
        public ulong UserId { get; private set; } = userId;
        public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
        public GenderType Gender { get; private set; }
        public int DebutYearStart { get; private set; }
        public int DebutYearEnd { get; private set; }
        public Dictionary<int, FileAttachment> IdolWithImage { get; private set; } = [];
        public List<int> Ranking { get; private set; } = [];

        public void SetGender(GenderChoiceEnum gender)
        {
            Gender = gender == GenderChoiceEnum.Female ?
                GenderType.Female :
                gender == GenderChoiceEnum.Male ?
                    GenderType.Male :
                    GenderType.None;
        }

        public void SetDebut(string[] chosenYears)
        {
            DebutYearStart = int.TryParse(chosenYears[0], out int start) ? start : 0;
            DebutYearEnd = int.TryParse(chosenYears[1], out int end) ? end : 0;
        }
    }
}
