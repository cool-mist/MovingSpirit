using System;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class CachedTokenProvider : IM2MTokenProvider
    {
        private IAccessToken accessToken;
        private readonly object accessTokenLock = new object();
        private readonly IM2MTokenProvider innerTokenProvider;

        internal CachedTokenProvider(IM2MTokenProvider innerTokenProvider)
        {
            this.innerTokenProvider = innerTokenProvider;
            DoRefreshOnce();
            _ = SchedulePeriodicRefresh();
        }

        public Task<IAccessToken> GetAccessToken()
        {
            return Task.FromResult(accessToken);
        }

        private async Task SchedulePeriodicRefresh()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(30));
                DoRefreshOnce();
            }
        }

        private void DoRefreshOnce()
        {
            lock (accessTokenLock)
            {
                accessToken = innerTokenProvider.GetAccessToken().GetAwaiter().GetResult();
            }
        }
    }
}
