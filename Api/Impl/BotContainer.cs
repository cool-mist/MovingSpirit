using Microsoft.Extensions.DependencyInjection;
using System;

namespace MovingSpirit.Api.Impl
{
    public static class BotContainer
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
            string commandTimeoutInSeconds = Environment.GetEnvironmentVariable("MS_COMMAND_TIMEOUT");

            ISpotTokenProvider innerTokenProvider = new M2MTokenProvider(clientId, clientSecret, audience, tokenUrl);
            ISpotTokenProvider cachedTokenProvider = new CachedTokenProvider(innerTokenProvider);
            ISpotController spotController = new SpotController(cachedTokenProvider, apiBaseUrl);
            IMinecraftClient minecraftClient = new MinecraftClient(serverName);
            TimeSpan commandTimeout = TimeSpan.FromSeconds(int.Parse(commandTimeoutInSeconds));

            ICommandHandler commandHandler = new CommandHandler(spotController, minecraftClient, commandTimeout);
            ICommandResponder commandResponder = new CommandResponder();

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(commandHandler)
                .AddSingleton(commandResponder)
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
