using System;

namespace MovingSpirit.Api.Impl
{
    public class BotConfig : IBotConfig
    {
        public BotConfig()
        {
            MinecraftServerName = Environment.GetEnvironmentVariable("MS_MCSERVER");
            ClientId = Environment.GetEnvironmentVariable("MS_CLIENT_ID");
            ClientSecret = Environment.GetEnvironmentVariable("MS_CLIENT_SECRET");
            TokenAudience = Environment.GetEnvironmentVariable("MS_LOGIN_AUDIENCE");
            TokenBaseUrl = Environment.GetEnvironmentVariable("MS_LOGIN_AUTHORITY");
            SpotApiBaseUrl = Environment.GetEnvironmentVariable("MS_SPOT_API");
            CommandTimeoutInSeconds = Environment.GetEnvironmentVariable("MS_COMMAND_TIMEOUT");
            DeleteAfterInSeconds = Environment.GetEnvironmentVariable("MS_RESPONSE_DELETE_AFTER");
        }

        public string MinecraftServerName { get; }

        public string SpotApiBaseUrl { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string TokenBaseUrl { get; }

        public string TokenAudience { get; }

        public string CommandTimeoutInSeconds { get; }

        public string DeleteAfterInSeconds { get; }

    }
}
