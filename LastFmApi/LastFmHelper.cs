namespace LastFmApi
{
    public class LastFmHelper
    {
        #region Input checks
        public static void LastfmParameterCheck(ref string[] parameters, bool defaultLimitTo10 = true)
        {
            string[] outarray;

            switch (parameters.Length)
            {
                //If both parameters are given, we check if they are in the right order
                //If only one of them is given, we figure out the other and give default value to the other
                case 2:
                {
                    outarray = int.TryParse(parameters[0], out _) && LastfmTimePeriod(parameters[1], out parameters[1])
                        ? ([parameters[0], parameters[1]])
                        : int.TryParse(parameters[1], out _) && LastfmTimePeriod(parameters[0], out parameters[0])
                            ? ([parameters[1], parameters[0]])
                            : throw new Exception("Wrong input format!");
                    break;
                }
                case 1:
                {
                    outarray = int.TryParse(parameters[0], out _)
                        ? [parameters[0], "overall"]
                        : LastfmTimePeriod(parameters[0], out parameters[0])
                            ? ["10", parameters[0]]
                            : throw new Exception("Wrong input format!");
                    break;
                }
                case 0:
                {
                    outarray = ["10", "overall"];
                    break;
                }
                default:
                    throw new Exception("Too many or too few parameters!");
            }

            if (int.TryParse(outarray[0], out int limit) && limit > 0 && defaultLimitTo10)
            {
                outarray[0] = limit > 31 ? "10" : outarray[0];
            }

            parameters = outarray;
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
}
