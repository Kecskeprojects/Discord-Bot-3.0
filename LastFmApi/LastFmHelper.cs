namespace LastFmApi
{
    public class LastFmHelper
    {
        #region Input checks
        //Checks the inputs for certain lastfm commands, so that they are ordered properly
        public static void LastfmParameterCheck(ref string[] parameters, bool defaultLimitTo10 = true)
        {
            string[] outarray = ["", ""]; ;

            switch (parameters.Length)
            {
                //If both parameters are given, we check if they are in the right order
                case 2:
                    {
                        //If first is number and second is part of array, input was in correct order
                        if (int.TryParse(parameters[0], out _) && LastfmTimePeriod(parameters[1], out parameters[1]))
                        {
                            outarray = [parameters[0], parameters[1]];
                        }
                        //If first is part of array and second is number, input is switched
                        else if (int.TryParse(parameters[1], out _) && LastfmTimePeriod(parameters[0], out parameters[0]))
                        {
                            outarray = [parameters[1], parameters[0]];
                        }
                        //Else the input was wrong
                        else throw new Exception("Wrong input format!");
                        break;
                    }
                //If only one of them is given, we figure out the other and give default value to the other
                case 1:
                    {
                        //If parameter is number, we give the other default value
                        if (int.TryParse(parameters[0], out _))
                        {
                            outarray[0] = parameters[0];
                            outarray[1] = "overall";
                        }
                        //If parameter is part of array, we give the other default value
                        else if (LastfmTimePeriod(parameters[0], out parameters[0]))
                        {
                            outarray[0] = "10";
                            outarray[1] = parameters[0];
                        }
                        //Else the input was wrong
                        else throw new Exception("Wrong input format!");
                        break;
                    }
                //If no parameters are given, we give default values
                case 0:
                    {
                        outarray[0] = "10";
                        outarray[1] = "overall";
                        break;
                    }
                //In every other case, we throw an error
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
