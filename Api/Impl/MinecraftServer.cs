using static MovingSpirit.Api.Impl.SlpTcpClient;

namespace MovingSpirit.Api.Impl
{
    internal class MinecraftServer : IMinecraftServer
    {
        public string Hostname { get; set; }

        public string Version { get; set; }

        public string Motd { get; set; }

        public int MaxPlayers { get; set; }

        public int OnlinePlayers { get; set; }

        public bool Online { get; set; }

        public string Icon { get; set; }

        public PingPayload Ping { get; set; }
    }
}
