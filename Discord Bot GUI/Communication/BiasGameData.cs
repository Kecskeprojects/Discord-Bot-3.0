using Discord;
using Discord_Bot.Enums;
using Discord_Bot.Services.Models.Twitter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public List<int[]> Pairs { get; private set; }
        public int CurrentPair { get; private set; }

        public Stack<int> Ranking { get; private set; } = [];

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
            int date1 = int.TryParse(chosenYears[0], out int start) ? start : 0;
            int date2 = int.TryParse(chosenYears[1], out int end) ? end : 0;
            DebutYearStart = date1 > date2 ? date2 : date1;
            DebutYearEnd = date1 > date2 ? date1 : date2;
        }

        public void AddImage(int idolId, Stream stream, string fileName)
        {
            IdolWithImage.TryAdd(idolId, new FileAttachment(stream, fileName));
        }

        public void CreatePairs()
        {
            int[] keys = [.. IdolWithImage.Keys.OrderBy(x => Guid.NewGuid())];
            CurrentPair = 0;
            Pairs = keys.Chunk(2).ToList();
        }

        internal void RemoveItem(int idolId)
        {
            Ranking.Push(idolId);

            IdolWithImage.Remove(idolId, out _);

            CurrentPair++;
        }
    }
}
