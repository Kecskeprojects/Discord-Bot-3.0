using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services;
public interface IMusicBrainzAPI
{
    Task<string> GetArtistSpotifyUrlAsync(string mbid);
}
