namespace Discord_Bot.Enums;
public enum SearchResultEnum
{
    YoutubeNotFound = 0,
    SpotifyNotFound = 1,
    YoutubeFoundVideo = 2,
    SpotifyVideoFound = 3,
    YoutubePlaylistFound = 4,
    SpotifyPlaylistFound = 5,
    SpotifyFoundYoutubeNotFound = 6,
    YoutubeSearchNotFound = 7
}
public static class SearchResultEnumExtension
{
    public static string ToMessageString(this SearchResultEnum result)
    {
        return result switch
        {
            SearchResultEnum.YoutubeNotFound => "No youtube video found or it is unlisted/private!",
            SearchResultEnum.SpotifyNotFound => "No results found on spotify!",
            SearchResultEnum.YoutubeFoundVideo => "Youtube result found!",
            SearchResultEnum.SpotifyVideoFound => "Spotify result found!",
            SearchResultEnum.YoutubePlaylistFound => "Youtube playlist added!",
            SearchResultEnum.SpotifyPlaylistFound => "Spotify playlist/album added!",
            SearchResultEnum.SpotifyFoundYoutubeNotFound => "Result found on spotify, but no youtube video/playlist found or it is unlisted/private!!",
            SearchResultEnum.YoutubeSearchNotFound => "Youtube video/playlist not found!",
            _ => "Unexpected result for search!"
        };
    }
}
