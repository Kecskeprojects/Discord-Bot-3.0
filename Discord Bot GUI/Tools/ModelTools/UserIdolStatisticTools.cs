using Discord_Bot.Database.Models;

namespace Discord_Bot.Tools.ModelTools;
public static class UserIdolStatisticTools
{
    public static void AddRanking(UserIdolStatistic stat, int pos)
    {
        switch (pos)
        {
            case 1:
            {
                stat.Placed1++;
                break;
            }
            case 2:
            {
                stat.Placed2++;
                break;
            }
            case 3:
            {
                stat.Placed3++;
                break;
            }
            case >= 4 and <= 7:
            {
                stat.Placed4++;
                break;
            }
            case >= 8 and <= 16:
            {
                stat.Placed5++;
                break;
            }
            default:
                break;
        }
    }
}
