namespace MovingSpirit.Api
{
    public interface IBotConfig
    {
        public string MinecraftServerName { get; }

        public string SpotApiBaseUrl { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string TokenBaseUrl { get; }

        public string TokenAudience { get; }

        public string CommandTimeoutInSeconds { get; }
    }
}
