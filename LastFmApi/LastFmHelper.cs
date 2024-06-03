using LastFmApi.Communication;

namespace LastFmApi;

public class LastFmHelper
{
    #region Input checks
    public static InputParameters LastfmParameterCheck(string[] parameters, bool defaultLimitTo10 = true)
    {
        InputParameters input = new();
        switch (parameters.Length)
        {
            //If both parameters are given, we check if they are in the right order
            //If only one of them is given, we figure out the other and give default value to the other
            case 2:
            {
                if (int.TryParse(parameters[0], out int limit) && LastfmTimePeriod(parameters[1], out string period))
                {
                    input.Limit = limit;
                    input.Period = period;
                }
                else if (int.TryParse(parameters[1], out limit) && LastfmTimePeriod(parameters[0], out period))
                {
                    input.Limit = limit;
                    input.Period = period;
                }
                else
                {
                    throw new Exception("Wrong input format!");
                }
                break;
            }
            case 1:
            {
                if(int.TryParse(parameters[0], out int limit))
                {
                    input.Limit = limit;
                    input.Period = "overall";
                }
                else if(LastfmTimePeriod(parameters[0], out string period))
                {
                    input.Limit = 10;
                    input.Period = period;
                }
                else
                {
                    throw new Exception("Wrong input format!");
                }
                break;
            }
            case 0:
            {
                input.Limit = 10;
                input.Period = "overall";
                break;
            }
            default:
                throw new Exception("Too many or too few parameters!");
        }
        
        if(defaultLimitTo10 && input.Limit > 30)
        {
            input.Limit = 10;
        }
        return input;
    }

    //Simple converter to lastfm API accepted formats
    public static bool LastfmTimePeriod(string input, out string period)
    {
        period = input.ToLower() switch
        {
            "overall" or "alltime" or "all" => "overall",
            "week" or "1week" or "7day" or "7days" => "7day",
            "30day" or "30days" or "month" or "1month" or "1months" => "1month",
            "quarter" or "3month" or "3months" => "3month",
            "half" or "6month" or "6months" => "6month",
            "year" or "1year" or "12month" or "12months" => "12month",
            _ => ""
        };

        return period != "";
    }
    #endregion
}
