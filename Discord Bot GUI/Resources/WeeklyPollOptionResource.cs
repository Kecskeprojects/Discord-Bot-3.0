namespace Discord_Bot.Resources;
public class WeeklyPollOptionResource
{
    public int WeeklyPollId { get; set; }
    public int? WeeklyPollOptionId { get; set; }
    public int? WeeklyPollOptionPresetId { get; set; }
    public byte OrderNumber { get; set; }
    public string Title { get; set; }
}
