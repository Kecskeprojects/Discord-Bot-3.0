namespace LastFmApi.Communication
{
    public class UserBasedRequestItem(string type, string username, string apiKey)
    {
        public UserBasedRequestItem(string type, string username, string apiKey, int? limit, int? page, string period) : this(type, username, apiKey)
        {
            Limit = limit;
            Page = page;
            Period = period;
        }

        public string Type { get; set; } = type;
        public string Username { get; set; } = username;
        public string ApiKey { get; set; } = apiKey;
        public int? Limit { get; set; }
        public int? Page { get; set; }
        public string Period { get; set; }
    }
}
