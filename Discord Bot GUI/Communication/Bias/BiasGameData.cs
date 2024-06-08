using Discord;
using Discord_Bot.Enums;
using Discord_Bot.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Discord_Bot.Communication.Bias;

public class BiasGameData
{
    public BiasGameData(ulong userId)
    {
        UserId = userId;
        Resource.winner_bracket.Save(WinnerBracket, System.Drawing.Imaging.ImageFormat.Png);
    }

    public ulong UserId { get; private set; }
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public GenderEnum Gender { get; set; }
    public int DebutYearStart { get; private set; }
    public int DebutYearEnd { get; private set; }

    public Dictionary<int, FileAttachment> IdolWithImage { get; private set; } = [];
    public List<int[]> Pairs { get; private set; }
    public int CurrentPair { get; private set; }
    public int CurrentRound { get; private set; } = 0;

    public Stack<int> Ranking { get; private set; } = [];
    public MemoryStream WinnerBracket { get; set; } = new MemoryStream();

    public ulong MessageId { get; set; }//This is only stored so if the game is stopped, the embed can be deleted

    public void SetGender(GenderEnum gender)
    {
        Gender = gender == GenderEnum.Female ?
            GenderEnum.Female :
            gender == GenderEnum.Male ?
                GenderEnum.Male :
                GenderEnum.NotSpecified;
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
        CurrentPair = 0;
        CurrentRound++;
        Pairs = IdolWithImage.Keys.Chunk(2).ToList();
    }

    public void RemoveItem(int idolId)
    {
        Ranking.Push(idolId);

        IdolWithImage.Remove(idolId, out _);

        CurrentPair++;
    }

    public void FinalizeData()
    {
        Ranking.Push(IdolWithImage.Keys.First());
        CurrentPair = 0;
        CurrentRound++;
        Pairs.Clear();
    }
}
