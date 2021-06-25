using System;

namespace MovingSpirit.Api.Impl
{
    public class BotConfig : IBotConfig
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string audience;
        private readonly string tokenUrl;
        private readonly string apiBaseUrl;
        private readonly string serverName;
        private readonly string commandTimeoutInSeconds;

        public BotConfig()
        {
            this.clientId = Environment.GetEnvironmentVariable("MS_CLIENT_ID");
            this.clientSecret = Environment.GetEnvironmentVariable("MS_CLIENT_SECRET");
            this.audience = Environment.GetEnvironmentVariable("MS_LOGIN_AUDIENCE");
            this.tokenUrl = Environment.GetEnvironmentVariable("MS_LOGIN_AUTHORITY");
            this.apiBaseUrl = Environment.GetEnvironmentVariable("MS_SPOT_API");
            this.serverName = Environment.GetEnvironmentVariable("MS_MCSERVER");
            this.commandTimeoutInSeconds = Environment.GetEnvironmentVariable("MS_COMMAND_TIMEOUT");
        }

        public string MinecraftServerName => serverName;

        public string SpotApiBaseUrl => apiBaseUrl;

        public string ClientId => clientId;

        public string ClientSecret => clientSecret;

        public string TokenBaseUrl => tokenUrl;

        public string TokenAudience => audience;

        public string CommandTimeoutInSeconds => commandTimeoutInSeconds;

    }
}
