using Microsoft.Extensions.DependencyInjection;
using System;

namespace MovingSpirit.Api.Impl
{
    public static class MovingSpiritContainer
    {
        public static IServiceProvider CreateServiceProvider()
        {
            // TODO : Configs
            string clientId = Environment.GetEnvironmentVariable("MS_CLIENT_ID");
            string clientSecret = Environment.GetEnvironmentVariable("MS_CLIENT_SECRET");
            string audience = Environment.GetEnvironmentVariable("MS_LOGIN_AUDIENCE");
            string tokenUrl = Environment.GetEnvironmentVariable("MS_LOGIN_AUTHORITY");
            string apiBaseUrl = Environment.GetEnvironmentVariable("MS_SPOT_API");
            string serverName = Environment.GetEnvironmentVariable("MS_MCSERVER");

            IM2MTokenProvider innerTokenProvider = new M2MTokenProvider(clientId, clientSecret, audience, tokenUrl);
            IM2MTokenProvider cachedTokenProvider = new CachedTokenProvider(innerTokenProvider);
            ISpotController spotController = new SpotController(cachedTokenProvider, apiBaseUrl);
            IMinecraftServerClient mcSrvrStatClient = new MinecraftServerClient(serverName);

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(cachedTokenProvider)
                .AddSingleton(spotController)
                .AddSingleton(mcSrvrStatClient)
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
