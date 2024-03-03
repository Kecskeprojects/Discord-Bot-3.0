using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface IBiasDatabaseService
    {
        Task RunUpdateBiasDataAsync();
    }
}
