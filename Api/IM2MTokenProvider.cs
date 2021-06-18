using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface IM2MTokenProvider
    {
        Task<IAccessToken> GetAccessToken();
    }
}
