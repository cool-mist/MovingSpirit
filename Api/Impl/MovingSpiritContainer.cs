﻿using Microsoft.Extensions.DependencyInjection;
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
            string commandTimeoutInSeconds = Environment.GetEnvironmentVariable("MS_COMMAND_TIMEOUT");

            IM2MTokenProvider innerTokenProvider = new M2MTokenProvider(clientId, clientSecret, audience, tokenUrl);
            IM2MTokenProvider cachedTokenProvider = new CachedTokenProvider(innerTokenProvider);
            ISpotController spotController = new SpotController(cachedTokenProvider, apiBaseUrl);
            IMinecraftServerClient mcSrvrStatClient = new MinecraftServerClient(serverName);
            ICommandTimeout commandTimeout = new CommandTimeout(TimeSpan.FromSeconds(int.Parse(commandTimeoutInSeconds)));

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(cachedTokenProvider)
                .AddSingleton(spotController)
                .AddSingleton(mcSrvrStatClient)
                .AddSingleton(commandTimeout)
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
