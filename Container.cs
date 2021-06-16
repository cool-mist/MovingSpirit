using Microsoft.Extensions.DependencyInjection;
using MovingSpirit.Api;
using System;

namespace MovingSpirit
{
    public static class Container
    {
        public static IServiceProvider CreateServiceProvider()
        {
            // TODO : Configs
            string clientId = Environment.GetEnvironmentVariable("MS_CLIENTID");
            string clientSecret = Environment.GetEnvironmentVariable("MS_CLIENTSECRET");
            string audience = "https://api.vanilla.nean.dev";
            string tokenUrl = "https://nean.us.auth0.com/oauth/token";
            string apiBaseUrl = "https://api.vanilla.nean.dev/";

            IM2MTokenProvider tokenProvider = new M2MTokenProvider(clientId, clientSecret, audience, tokenUrl);
            ISpotController spotController = new SpotController(tokenProvider, apiBaseUrl);

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(tokenProvider)
                .AddSingleton(spotController)
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
