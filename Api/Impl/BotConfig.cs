using System;

namespace MovingSpirit.Api.Impl
{
    public class BotConfig : IBotConfig
    {
        public string MinecraftServerName => GetConfigValue("MS_MCSERVER");

        public string SpotApiBaseUrl => GetConfigValue("MS_SPOT_API");

        public string ClientId => GetConfigValue("MS_CLIENT_ID");

        public string ClientSecret => GetConfigValue("MS_CLIENT_SECRET");

        public string BotToken => GetConfigValue("MS_TOKEN");

        public string TokenBaseUrl => GetConfigValue("MS_LOGIN_AUTHORITY");

        public string TokenAudience => GetConfigValue("MS_LOGIN_AUDIENCE");

        public string CommandTimeoutInSeconds => GetConfigValue("MS_COMMAND_TIMEOUT");

        public string DeleteAfterInSeconds => GetConfigValue("MS_RESPONSE_DELETE_AFTER");

        public string HistoryChannelId => GetConfigValue("MS_HISTORY_CHANNEL_ID");

        public string ServerStatusChannelId => GetConfigValue("MS_SERVER_STATUS_CHANNEL_ID");

        private static string GetConfigValue(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }
}
